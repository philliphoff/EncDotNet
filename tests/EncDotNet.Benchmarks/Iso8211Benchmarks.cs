using BenchmarkDotNet.Attributes;
using EncDotNet.Iso8211;

namespace EncDotNet.Benchmarks;

/// <summary>
/// Benchmarks for the ISO 8211 layer: the token reader, the document reader, the DDR reader,
/// subfield decoding and the writer.
/// </summary>
[MemoryDiagnoser]
public class Iso8211Benchmarks
{
    private byte[] _bytes = null!;
    private Iso8211Document _document = null!;
    private Iso8211Record _ddrRecord = null!;
    private Iso8211FieldDefinition _sg2dDefinition = null!;
    private Iso8211Field[] _sg2dFields = null!;

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
        _document = Iso8211DocumentReader.Read(_bytes);
        _ddrRecord = _document.DataDescriptiveRecord!;
        _sg2dDefinition = Iso8211DataDescriptiveRecordReader.Read(_ddrRecord).GetFieldDefinition("SG2D")!;
        _sg2dFields = [.. _document.DataRecords.SelectMany(r => r.Fields).Where(f => f.Tag == "SG2D")];
    }

    [BenchmarkCategory(BenchmarkCategories.Core)]
    [Benchmark(Description = "Iso8211Reader: read all tokens")]
    public int Iso8211Reader_ReadAllTokens()
    {
        var reader = new Iso8211Reader(_bytes);
        var count = 0;
        while (reader.Read())
        {
            count++;
        }

        return count;
    }

    [Benchmark(Description = "Iso8211Reader: skip all records")]
    public int Iso8211Reader_SkipAllRecords()
    {
        var reader = new Iso8211Reader(_bytes);
        var count = 0;
        while (reader.Read())
        {
            if (reader.TokenType == Iso8211TokenType.StartRecord)
            {
                reader.SkipRecord();
                count++;
            }
        }

        return count;
    }

    [BenchmarkCategory(BenchmarkCategories.Core)]
    [Benchmark(Description = "Iso8211DocumentReader.Read")]
    public Iso8211Document Iso8211DocumentReader_Read() => Iso8211DocumentReader.Read(_bytes);

    [Benchmark(Description = "Iso8211DataDescriptiveRecordReader.Read")]
    public Iso8211DataDescriptiveRecord DataDescriptiveRecordReader_Read() => Iso8211DataDescriptiveRecordReader.Read(_ddrRecord);

    [BenchmarkCategory(BenchmarkCategories.Core)]
    [Benchmark(Description = "Iso8211FieldReader: decode SG2D")]
    public long Iso8211FieldReader_DecodeCoordinates()
    {
        long sum = 0;
        foreach (var field in _sg2dFields)
        {
            var reader = new Iso8211FieldReader(_sg2dDefinition, field.Data);
            foreach (var group in reader.GetSubfieldGroups())
            {
                sum += group.GetSubfield<int>("YCOO") + group.GetSubfield<int>("XCOO");
            }
        }

        return sum;
    }

    [BenchmarkCategory(BenchmarkCategories.Core)]
    [Benchmark(Description = "Iso8211DocumentWriter.Write")]
    public byte[] Iso8211DocumentWriter_Write() => Iso8211DocumentWriter.Write(_document);
}
