using BenchmarkDotNet.Attributes;
using EncDotNet.S57;
using EncDotNet.S57.Charts;

namespace EncDotNet.Benchmarks;

/// <summary>
/// Benchmarks for the S-57 layer: reading a document and building a chart from it.
/// </summary>
[MemoryDiagnoser]
public class S57Benchmarks
{
    private byte[] _bytes = null!;
    private S57Document _document = null!;

    /// <summary>
    /// Gets or sets the cell to read.
    /// </summary>
    [ParamsSource(nameof(Inputs))]
    public BenchmarkInput Input { get; set; } = null!;

    /// <summary>
    /// Gets the cells to benchmark.
    /// </summary>
    public static IEnumerable<BenchmarkInput> Inputs => BenchmarkInput.All();

    [GlobalSetup]
    public void Setup()
    {
        _bytes = Input.Bytes;
        _document = S57DocumentReader.Read(_bytes);
    }

    [BenchmarkCategory(BenchmarkCategories.Core)]
    [Benchmark(Description = "S57DocumentReader.Read")]
    public S57Document S57DocumentReader_Read() => S57DocumentReader.Read(_bytes);

    [BenchmarkCategory(BenchmarkCategories.Core)]
    [Benchmark(Description = "S57Chart.FromDocument")]
    public S57Chart S57Chart_FromDocument() => S57Chart.FromDocument(_document);

    [Benchmark(Description = "Bytes to S57Chart")]
    public S57Chart BytesToChart() => S57Chart.FromDocument(S57DocumentReader.Read(_bytes));
}
