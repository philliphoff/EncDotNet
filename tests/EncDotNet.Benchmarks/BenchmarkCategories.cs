namespace EncDotNet.Benchmarks;

/// <summary>
/// Benchmark categories, for selecting a subset with <c>--anyCategories</c>.
/// </summary>
public static class BenchmarkCategories
{
    /// <summary>
    /// The benchmarks the pull request comparison runs: one per public reader, writer and
    /// model builder, covering the work an application does to load a chart.
    /// </summary>
    public const string Core = "Core";
}
