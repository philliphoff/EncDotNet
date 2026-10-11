using BenchmarkDotNet.Attributes;
using EncDotNet.S57;

namespace EncDotNet.Benchmarks;

/// <summary>
/// Benchmarks for applying update files (<c>.001</c>, <c>.002</c>, …) to a base cell.
/// </summary>
[MemoryDiagnoser]
public class S57UpdateBenchmarks
{
    private S57Document _base = null!;
    private S57Document[] _updates = null!;

    /// <summary>
    /// Gets or sets the cell to update.
    /// </summary>
    [ParamsSource(nameof(Inputs))]
    public BenchmarkInput Input { get; set; } = null!;

    /// <summary>
    /// Gets the cells that have update files.
    /// </summary>
    public static IEnumerable<BenchmarkInput> Inputs => BenchmarkInput.WithUpdates();

    [GlobalSetup]
    public void Setup()
    {
        _base = S57DocumentReader.Read(Input.Bytes);
        _updates = [.. Input.Updates.Select(u => S57DocumentReader.Read(u))];
    }

    [BenchmarkCategory(BenchmarkCategories.Core)]
    [Benchmark(Description = "S57Document.ApplyChanges (all updates)")]
    public S57Document ApplyChanges()
    {
        var document = _base;
        foreach (var update in _updates)
        {
            document = document.ApplyChanges(update);
        }

        return document;
    }
}
