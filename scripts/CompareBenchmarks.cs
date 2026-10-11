#!/usr/bin/env dotnet run

using System.Globalization;
using System.Text;
using System.Text.Json;

// ============================================================================
// CompareBenchmarks.cs — Compares BenchmarkDotNet results for a base and a head
// build and writes a Markdown report.
//
// Usage:
//   dotnet run scripts/CompareBenchmarks.cs -- --base <dir>... --head <dir>... [--threshold 10]
//
// Each <dir> is one round's BenchmarkDotNet artifacts folder; the script reads
// every *-report-full-compressed.json file under it. Pass the rounds in the
// order they ran: base and head round N are paired to judge whether a change
// is consistent or noise.
//
// Timings from shared CI runners are noisy, so this report is informational.
// A change is flagged only when it exceeds the threshold in every round.
// ============================================================================

var baseDirs = new List<string>();
var headDirs = new List<string>();
var threshold = 10.0;

List<string>? target = null;
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--base":
            target = baseDirs;
            break;
        case "--head":
            target = headDirs;
            break;
        case "--threshold":
            threshold = double.Parse(args[++i], CultureInfo.InvariantCulture);
            target = null;
            break;
        default:
            if (target is null)
            {
                Console.Error.WriteLine($"Unexpected argument: {args[i]}");
                return 1;
            }

            target.Add(args[i]);
            break;
    }
}

if (headDirs.Count == 0)
{
    Console.Error.WriteLine("Usage: CompareBenchmarks.cs --base <dir>... --head <dir>... [--threshold 10]");
    return 1;
}

var baseRounds = baseDirs.Select(LoadRound).ToList();
var headRounds = headDirs.Select(LoadRound).ToList();
var host = headRounds.SelectMany(r => r.Values).Select(r => r.Host).FirstOrDefault(h => h is not null);

var report = new StringBuilder();
report.AppendLine("## Benchmark comparison");
report.AppendLine();
report.AppendLine(
    $"Timings are from a shared runner and are **informational**; the allocation and retained-memory " +
    $"budgets in `tests/EncDotNet.PerformanceTests` are the gate. Base and head ran in {headRounds.Count} " +
    $"interleaved round(s) on the same machine. A change is flagged when it exceeds {threshold:F0}% in every round.");
if (host is not null)
{
    report.AppendLine();
    report.AppendLine($"Runner: {host}");
}

report.AppendLine();
report.AppendLine("| Benchmark | Input | Base | Head | Time | Allocated (base → head) | Allocated |");
report.AppendLine("|---|---|---:|---:|---:|---:|---:|");

var flagged = 0;
var keys = headRounds.SelectMany(r => r.Keys).Distinct().Order(StringComparer.Ordinal);
foreach (var key in keys)
{
    var head = headRounds.Where(r => r.ContainsKey(key)).Select(r => r[key]).ToList();
    var basis = baseRounds.Where(r => r.ContainsKey(key)).Select(r => r[key]).ToList();
    var first = head[0];

    if (basis.Count == 0)
    {
        report.AppendLine(
            $"| {first.Title} | {first.Input} | — | {FormatTime(Median(head.Select(h => h.MedianNs)))} | new | " +
            $"{FormatBytes(first.AllocatedBytes)} | new |");
        continue;
    }

    var baseTime = Median(basis.Select(b => b.MedianNs));
    var headTime = Median(head.Select(h => h.MedianNs));
    var change = (headTime / baseTime - 1) * 100;

    // Pair rounds in the order they ran; a real change shows in each pair.
    var roundChanges = basis.Zip(head, (b, h) => (h.MedianNs / b.MedianNs - 1) * 100).ToList();
    var consistent = roundChanges.All(c => c >= threshold) || roundChanges.All(c => c <= -threshold);
    var marker = consistent ? (change > 0 ? " ⚠️" : " ✅") : string.Empty;
    if (consistent)
    {
        flagged++;
    }

    var baseAllocated = basis[^1].AllocatedBytes;
    var headAllocated = head[^1].AllocatedBytes;
    var allocatedChange = baseAllocated == 0
        ? (headAllocated == 0 ? "0%" : "new")
        : $"{(headAllocated / (double)baseAllocated - 1) * 100:+0.0;-0.0;0.0}%";

    report.AppendLine(
        $"| {first.Title} | {first.Input} | {FormatTime(baseTime)} | {FormatTime(headTime)} | " +
        $"{change:+0.0;-0.0;0.0}%{marker} | {FormatBytes(baseAllocated)} → {FormatBytes(headAllocated)} | {allocatedChange} |");
}

foreach (var key in baseRounds.SelectMany(r => r.Keys).Distinct().Except(headRounds.SelectMany(r => r.Keys)).Order(StringComparer.Ordinal))
{
    var removed = baseRounds.First(r => r.ContainsKey(key))[key];
    report.AppendLine($"| {removed.Title} | {removed.Input} | {FormatTime(removed.MedianNs)} | — | removed | | |");
}

report.AppendLine();
report.AppendLine(flagged == 0
    ? "No timing change was consistent across rounds."
    : $"{flagged} timing change(s) were consistent across rounds: ⚠️ slower, ✅ faster. Confirm on a quiet machine before acting on them.");

Console.Write(report.ToString());
return 0;

static Dictionary<string, Result> LoadRound(string directory)
{
    var results = new Dictionary<string, Result>(StringComparer.Ordinal);
    if (!Directory.Exists(directory))
    {
        return results;
    }

    foreach (var file in Directory.GetFiles(directory, "*-report-full-compressed.json", SearchOption.AllDirectories))
    {
        using var json = JsonDocument.Parse(File.ReadAllText(file));
        var host = DescribeHost(json.RootElement);
        foreach (var benchmark in json.RootElement.GetProperty("Benchmarks").EnumerateArray())
        {
            if (!benchmark.TryGetProperty("Statistics", out var statistics) || statistics.ValueKind != JsonValueKind.Object)
            {
                continue; // The benchmark failed in this round.
            }

            var type = benchmark.GetProperty("Type").GetString()!;
            var method = benchmark.GetProperty("Method").GetString()!;
            var title = benchmark.GetProperty("MethodTitle").GetString()!.Trim('\'');
            var parameters = benchmark.GetProperty("Parameters").GetString() ?? string.Empty;
            var input = parameters.StartsWith("Input=", StringComparison.Ordinal) ? parameters["Input=".Length..] : parameters;
            var allocated = benchmark.TryGetProperty("Memory", out var memory)
                ? memory.GetProperty("BytesAllocatedPerOperation").GetInt64()
                : 0;

            results[$"{type}.{method}|{input}"] = new Result(
                title, input, statistics.GetProperty("Median").GetDouble(), allocated, host);
        }
    }

    return results;
}

static string? DescribeHost(JsonElement root)
{
    if (!root.TryGetProperty("HostEnvironmentInfo", out var info))
    {
        return null;
    }

    var processor = info.TryGetProperty("ProcessorName", out var p) ? p.GetString() : null;
    var runtime = info.TryGetProperty("RuntimeVersion", out var r) ? r.GetString() : null;
    var os = info.TryGetProperty("OsVersion", out var o) ? o.GetString() : null;
    return string.Join(", ", new[] { processor, os, runtime }.Where(s => !string.IsNullOrEmpty(s)));
}

static double Median(IEnumerable<double> values)
{
    var sorted = values.Order().ToArray();
    var middle = sorted.Length / 2;
    return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
}

static string FormatTime(double nanoseconds) => nanoseconds switch
{
    >= 1e9 => $"{nanoseconds / 1e9:F2} s",
    >= 1e6 => $"{nanoseconds / 1e6:F2} ms",
    >= 1e3 => $"{nanoseconds / 1e3:F2} μs",
    _ => $"{nanoseconds:F0} ns"
};

static string FormatBytes(long bytes) => bytes switch
{
    >= 1L << 20 => $"{bytes / (double)(1L << 20):F2} MB",
    >= 1L << 10 => $"{bytes / (double)(1L << 10):F1} KB",
    _ => $"{bytes} B"
};

internal sealed record Result(string Title, string Input, double MedianNs, long AllocatedBytes, string? Host);
