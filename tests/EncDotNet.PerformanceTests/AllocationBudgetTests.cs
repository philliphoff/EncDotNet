using EncDotNet.Iso8211;
using EncDotNet.S57;
using EncDotNet.S57.Charts;

namespace EncDotNet.PerformanceTests;

/// <summary>
/// Gates allocation regressions in the readers and writers against <see cref="PerformanceBudgets"/>.
/// </summary>
/// <remarks>
/// Allocated bytes are exact and identical between runs and machines, so unlike elapsed time
/// they can fail a build reliably on shared CI runners. Allocation also drives much of the CPU
/// cost of these APIs through garbage collection.
/// </remarks>
public class AllocationBudgetTests
{
    private static PerformanceCells.Inputs Cell => PerformanceCells.Harbour;

    [Fact]
    public void Iso8211Reader_ReadingAllTokens_DoesNotAllocate()
    {
        var bytes = Cell.Bytes;

        var allocated = MemoryMeter.AllocatedBytes(() =>
        {
            var reader = new Iso8211Reader(bytes);
            while (reader.Read())
            {
            }
        });

        PerformanceBudget.AssertAllocated("Iso8211Reader all tokens", allocated, PerformanceBudgets.Iso8211ReaderAllTokens);
    }

    [Fact]
    public void Iso8211DocumentReader_Read_StaysWithinBudget()
    {
        var bytes = Cell.Bytes;

        var allocated = MemoryMeter.AllocatedBytes(() => Iso8211DocumentReader.Read(bytes));

        PerformanceBudget.AssertAllocated("Iso8211DocumentReader.Read", allocated, PerformanceBudgets.Iso8211DocumentReaderRead);
    }

    [Fact]
    public void Iso8211DataDescriptiveRecordReader_Read_StaysWithinBudget()
    {
        var ddr = Cell.Iso8211Document.DataDescriptiveRecord!;

        var allocated = MemoryMeter.AllocatedBytes(() => Iso8211DataDescriptiveRecordReader.Read(ddr));

        PerformanceBudget.AssertAllocated(
            "Iso8211DataDescriptiveRecordReader.Read", allocated, PerformanceBudgets.Iso8211DataDescriptiveRecordReaderRead);
    }

    [Fact]
    public void Iso8211FieldReader_DecodingCoordinates_StaysWithinBudget()
    {
        // SG2D decoding is the single hottest path when reading a cell.
        var ddr = Iso8211DataDescriptiveRecordReader.Read(Cell.Iso8211Document.DataDescriptiveRecord!);
        var definition = ddr.GetFieldDefinition("SG2D")!;
        var fields = Cell.Iso8211Document.DataRecords
            .SelectMany(r => r.Fields)
            .Where(f => f.Tag == "SG2D")
            .ToArray();

        var allocated = MemoryMeter.AllocatedBytes(() =>
        {
            foreach (var field in fields)
            {
                var reader = new Iso8211FieldReader(definition, field.Data);
                foreach (var group in reader.GetSubfieldGroups())
                {
                    _ = group.GetSubfield<int>("YCOO");
                    _ = group.GetSubfield<int>("XCOO");
                }
            }
        });

        PerformanceBudget.AssertAllocated("Iso8211FieldReader SG2D", allocated, PerformanceBudgets.Iso8211FieldReaderCoordinates);
    }

    [Fact]
    public void Iso8211FieldReader_DecodingCoordinatesByOrdinal_StaysWithinBudget()
    {
        // The ordinal accessors are the allocation-free way to read a repeating group: the
        // name lookups happen once per field, and no group objects are created.
        var ddr = Iso8211DataDescriptiveRecordReader.Read(Cell.Iso8211Document.DataDescriptiveRecord!);
        var definition = ddr.GetFieldDefinition("SG2D")!;
        var fields = Cell.Iso8211Document.DataRecords
            .SelectMany(r => r.Fields)
            .Where(f => f.Tag == "SG2D")
            .ToArray();

        var allocated = MemoryMeter.AllocatedBytes(() =>
        {
            foreach (var field in fields)
            {
                var reader = new Iso8211FieldReader(definition, field.Data);
                var y = reader.GetOrdinal("YCOO");
                var x = reader.GetOrdinal("XCOO");
                for (var group = 0; group < reader.GroupCount; group++)
                {
                    _ = reader.GetSubfield<int>(group, y);
                    _ = reader.GetSubfield<int>(group, x);
                }
            }
        });

        PerformanceBudget.AssertAllocated(
            "Iso8211FieldReader SG2D by ordinal", allocated, PerformanceBudgets.Iso8211FieldReaderCoordinatesByOrdinal);
    }

    [Fact]
    public void Iso8211DocumentWriter_Write_StaysWithinBudget()
    {
        var document = Cell.Iso8211Document;

        var allocated = MemoryMeter.AllocatedBytes(() => Iso8211DocumentWriter.Write(document));

        PerformanceBudget.AssertAllocated("Iso8211DocumentWriter.Write", allocated, PerformanceBudgets.Iso8211DocumentWriterWrite);
    }

    [Fact]
    public void S57DocumentReader_Read_StaysWithinBudget()
    {
        var bytes = Cell.Bytes;

        var allocated = MemoryMeter.AllocatedBytes(() => S57DocumentReader.Read(bytes));

        PerformanceBudget.AssertAllocated("S57DocumentReader.Read", allocated, PerformanceBudgets.S57DocumentReaderRead);
    }

    [Fact]
    public void S57Document_ApplyChanges_StaysWithinBudget()
    {
        var baseDocument = Cell.S57Document;
        var updates = Cell.Updates;

        var allocated = MemoryMeter.AllocatedBytes(() =>
        {
            var document = baseDocument;
            foreach (var update in updates)
            {
                document = document.ApplyChanges(update);
            }
        });

        PerformanceBudget.AssertAllocated("S57Document.ApplyChanges x3", allocated, PerformanceBudgets.S57DocumentApplyChanges);
    }

    [Fact]
    public void S57Chart_FromDocument_StaysWithinBudget()
    {
        var document = Cell.S57Document;

        var allocated = MemoryMeter.AllocatedBytes(() => S57Chart.FromDocument(document));

        PerformanceBudget.AssertAllocated("S57Chart.FromDocument", allocated, PerformanceBudgets.S57ChartFromDocument);
    }

    [Fact]
    public void S57DocumentReader_Read_AllocatesInProportionToInputSize()
    {
        // A cell with four times the records should cost about four times as much per byte
        // read. A super-linear structure (a list scanned per record, say) breaks this.
        var small = PerformanceCells.Harbour;
        var large = PerformanceCells.HarbourX4;

        var smallPerByte = (double)MemoryMeter.AllocatedBytes(() => S57DocumentReader.Read(small.Bytes)) / small.Bytes.Length;
        var largePerByte = (double)MemoryMeter.AllocatedBytes(() => S57DocumentReader.Read(large.Bytes)) / large.Bytes.Length;

        var ratio = largePerByte / smallPerByte;
        TestContext.Current.TestOutputHelper?.WriteLine(
            $"Allocated per input byte: {smallPerByte:F1} (1x), {largePerByte:F1} (4x), ratio {ratio:F3}");
        Assert.InRange(ratio, 0.85, 1.15);
    }
}
