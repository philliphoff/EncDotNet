using BenchmarkDotNet.Attributes;
using EncDotNet.Iso8211;

namespace EncDotNet.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="Iso8211Reader"/> measuring raw token-reading throughput
/// against real NOAA ENC chart files.
/// </summary>
/// <remarks>
/// Set the <c>ENCDOTNET_ENC_ROOT</c> environment variable to the root directory
/// containing NOAA ENC files (*.000) before running.
/// </remarks>
[MemoryDiagnoser]
public class Iso8211ReaderBenchmarks
{
    private byte[][] _chartFiles = null!;
    private byte[] _singleChart = null!;

    [GlobalSetup]
    public void Setup()
    {
        var encRoot = Environment.GetEnvironmentVariable("ENCDOTNET_ENC_ROOT")
            ?? throw new InvalidOperationException(
                "Set the ENCDOTNET_ENC_ROOT environment variable to the NOAA ENC root directory.");

        var files = Directory.GetFiles(encRoot, "*.000", SearchOption.AllDirectories);

        if (files.Length == 0)
        {
            throw new InvalidOperationException(
                $"No .000 files found under '{encRoot}'.");
        }

        _chartFiles = files.Select(File.ReadAllBytes).ToArray();
        _singleChart = _chartFiles.MaxBy(f => f.Length)!;
    }

    [Benchmark(Description = "ReadAllTokens (single largest chart)")]
    public int ReadAllTokens_SingleChart()
    {
        var reader = new Iso8211Reader(_singleChart);
        int count = 0;
        while (reader.Read()) count++;
        return count;
    }

    [Benchmark(Baseline = true, Description = "ReadAllTokens (all charts)")]
    public int ReadAllTokens_AllCharts()
    {
        int count = 0;
        foreach (var file in _chartFiles)
        {
            var reader = new Iso8211Reader(file);
            while (reader.Read()) count++;
        }
        return count;
    }

    [Benchmark(Description = "SkipAllRecords (all charts)")]
    public int SkipAllRecords_AllCharts()
    {
        int count = 0;
        foreach (var file in _chartFiles)
        {
            var reader = new Iso8211Reader(file);
            while (reader.Read())
            {
                if (reader.TokenType == Iso8211TokenType.StartRecord)
                {
                    reader.SkipRecord();
                    count++;
                }
            }
        }
        return count;
    }

    [Benchmark(Description = "ReadFields, skip directory (all charts)")]
    public int ReadFields_SkipDirectory_AllCharts()
    {
        int count = 0;
        foreach (var file in _chartFiles)
        {
            var reader = new Iso8211Reader(file);
            while (reader.Read())
            {
                if (reader.TokenType == Iso8211TokenType.StartRecord)
                {
                    reader.SkipDirectory();
                }
                else if (reader.TokenType == Iso8211TokenType.Field)
                {
                    count++;
                }
            }
        }
        return count;
    }

    [Benchmark(Description = "ReadFields with tag string (all charts)")]
    public int ReadFields_WithTagString_AllCharts()
    {
        int count = 0;
        foreach (var file in _chartFiles)
        {
            var reader = new Iso8211Reader(file);
            while (reader.Read())
            {
                if (reader.TokenType == Iso8211TokenType.StartRecord)
                {
                    reader.SkipDirectory();
                }
                else if (reader.TokenType == Iso8211TokenType.Field)
                {
                    _ = reader.GetTagString();
                    count++;
                }
            }
        }
        return count;
    }

    [Benchmark(Description = "ReadFields with span tag compare (all charts)")]
    public int ReadFields_WithSpanTagCompare_AllCharts()
    {
        int count = 0;
        foreach (var file in _chartFiles)
        {
            var reader = new Iso8211Reader(file);
            while (reader.Read())
            {
                if (reader.TokenType == Iso8211TokenType.StartRecord)
                {
                    reader.SkipDirectory();
                }
                else if (reader.TokenType == Iso8211TokenType.Field)
                {
                    _ = reader.CurrentTag.SequenceEqual("VRID"u8);
                    count++;
                }
            }
        }
        return count;
    }
}
