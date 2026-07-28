using BenchmarkDotNet.Attributes;
using EncDotNet.Iso8211;
using EncDotNet.S57;

namespace EncDotNet.Benchmarks;

/// <summary>
/// Benchmarks comparing the cost of each parsing layer:
/// raw <see cref="Iso8211Reader"/> vs <see cref="Iso8211DocumentReader"/> vs <see cref="S57DocumentReader"/>.
/// </summary>
[MemoryDiagnoser]
public class ParsingLayerBenchmarks
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

    [Benchmark(Baseline = true, Description = "Iso8211Reader tokens (single chart)")]
    public int Iso8211Reader_SingleChart()
    {
        var reader = new Iso8211Reader(_singleChart);
        int count = 0;
        while (reader.Read()) count++;
        return count;
    }

    [Benchmark(Description = "Iso8211DocumentReader (single chart)")]
    public Iso8211Document Iso8211DocumentReader_SingleChart()
    {
        return Iso8211DocumentReader.Read(_singleChart);
    }

    [Benchmark(Description = "S57DocumentReader (single chart)")]
    public S57Document S57DocumentReader_SingleChart()
    {
        return S57DocumentReader.Read(_singleChart);
    }
}
