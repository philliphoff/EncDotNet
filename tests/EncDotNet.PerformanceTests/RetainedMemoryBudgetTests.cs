using EncDotNet.Iso8211;
using EncDotNet.S57;
using EncDotNet.S57.Charts;

namespace EncDotNet.PerformanceTests;

/// <summary>
/// Gates growth in the memory the parsed models hold, which bounds how many charts an
/// application such as a chart viewer can keep loaded.
/// </summary>
[Collection(nameof(RetainedMemoryCollection))]
public class RetainedMemoryBudgetTests
{
    private static PerformanceCells.Inputs Cell => PerformanceCells.Harbour;

    [Fact]
    public void Iso8211Document_StaysWithinBudget()
    {
        var bytes = Cell.Bytes;

        var retained = MemoryMeter.RetainedBytes(() => Iso8211DocumentReader.Read(bytes));

        PerformanceBudget.AssertRetained("Iso8211Document", retained, PerformanceBudgets.Iso8211DocumentRetained);
    }

    [Fact]
    public void S57Document_StaysWithinBudget()
    {
        var bytes = Cell.Bytes;

        var retained = MemoryMeter.RetainedBytes(() => S57DocumentReader.Read(bytes));

        PerformanceBudget.AssertRetained("S57Document", retained, PerformanceBudgets.S57DocumentRetained);
    }

    [Fact]
    public void S57Chart_StaysWithinBudget()
    {
        // Measure the whole load path. The chart keeps the records' attributes, pointers and
        // coordinates but not the document itself, so it can retain less than the document.
        var bytes = Cell.Bytes;

        var retained = MemoryMeter.RetainedBytes(() => S57Chart.FromDocument(S57DocumentReader.Read(bytes)));

        PerformanceBudget.AssertRetained("S57Chart", retained, PerformanceBudgets.S57ChartRetained);
    }
}
