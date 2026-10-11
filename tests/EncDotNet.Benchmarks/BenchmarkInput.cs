using EncDotNet.Testing;

namespace EncDotNet.Benchmarks;

/// <summary>
/// A cell to benchmark: a base cell (<c>.000</c>) and its updates, if any.
/// </summary>
/// <remarks>
/// Inputs load lazily, because BenchmarkDotNet runs each case in its own process and only
/// that case's input needs to be built.
/// </remarks>
public sealed class BenchmarkInput
{
    /// <summary>
    /// The environment variable naming a directory of real ENC cells to benchmark
    /// alongside the synthetic ones. Every <c>*.000</c> file under it, at any depth,
    /// becomes an input, with the numbered update files next to it.
    /// </summary>
    public const string EncRootVariable = "ENCDOTNET_ENC_ROOT";

    /// <summary>
    /// The environment variable listing which synthetic profiles to benchmark, comma-separated
    /// (for example <c>Large</c>). Unset means all of them; <c>none</c> means none.
    /// </summary>
    public const string SyntheticVariable = "ENCDOTNET_BENCHMARK_SYNTHETIC";

    private const int SyntheticUpdateCount = 3;

    private readonly Func<byte[]> _loadBase;
    private readonly Func<IReadOnlyList<byte[]>> _loadUpdates;
    private byte[]? _bytes;
    private IReadOnlyList<byte[]>? _updates;

    private BenchmarkInput(string name, bool hasUpdates, Func<byte[]> loadBase, Func<IReadOnlyList<byte[]>> loadUpdates)
    {
        Name = name;
        HasUpdates = hasUpdates;
        _loadBase = loadBase;
        _loadUpdates = loadUpdates;
    }

    /// <summary>
    /// Gets the name shown in benchmark results: the profile name for a synthetic cell
    /// (prefixed <c>Synthetic</c>), or the cell name for a real one.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the cell has update files.
    /// </summary>
    public bool HasUpdates { get; }

    /// <summary>
    /// Gets the base cell's bytes.
    /// </summary>
    public byte[] Bytes => _bytes ??= _loadBase();

    /// <summary>
    /// Gets the bytes of the update files, in order.
    /// </summary>
    public IReadOnlyList<byte[]> Updates => _updates ??= _loadUpdates();

    /// <summary>
    /// Gets the synthetic cells selected by <see cref="SyntheticVariable"/>, followed by any
    /// real cells under <see cref="EncRootVariable"/>.
    /// </summary>
    public static IEnumerable<BenchmarkInput> All()
    {
        var selected = Environment.GetEnvironmentVariable(SyntheticVariable)?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var profiles = new[] { SyntheticCellProfile.Harbour, SyntheticCellProfile.Approach, SyntheticCellProfile.Large }
            .Where(p => selected is null || selected.Contains(p.Name, StringComparer.OrdinalIgnoreCase));

        foreach (var profile in profiles)
        {
            yield return new BenchmarkInput(
                $"Synthetic{profile.Name}",
                hasUpdates: true,
                () => SyntheticS57Cell.Create(profile),
                () => [.. Enumerable.Range(1, SyntheticUpdateCount).Select(n => SyntheticS57Cell.CreateUpdate(profile, n))]);
        }

        var root = Environment.GetEnvironmentVariable(EncRootVariable);
        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
        {
            yield break;
        }

        foreach (var path in Directory.GetFiles(root, "*.000", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            yield return new BenchmarkInput(
                Path.GetFileNameWithoutExtension(path),
                UpdatePaths(path).Any(),
                () => File.ReadAllBytes(path),
                () => [.. UpdatePaths(path).Select(File.ReadAllBytes)]);
        }
    }

    /// <summary>
    /// Gets the inputs that have update files.
    /// </summary>
    public static IEnumerable<BenchmarkInput> WithUpdates() => All().Where(input => input.HasUpdates);

    /// <inheritdoc/>
    public override string ToString() => Name;

    private static IEnumerable<string> UpdatePaths(string basePath)
    {
        var directory = Path.GetDirectoryName(basePath);
        if (string.IsNullOrEmpty(directory))
        {
            return [];
        }

        var stem = Path.GetFileNameWithoutExtension(basePath);
        return Enumerable.Range(1, 999)
            .Select(n => Path.Combine(directory, $"{stem}.{n:D3}"))
            .TakeWhile(File.Exists);
    }
}
