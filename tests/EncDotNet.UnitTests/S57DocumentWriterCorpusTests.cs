using System.Globalization;
using System.Text;
using EncDotNet.Iso8211;
using EncDotNet.S57;
using EncDotNet.S57.ExchangeSets;

namespace EncDotNet.UnitTests;

/// <summary>
/// Round-trips real cells and catalogues through the S-57 writers. Real cells aren't
/// committed, so these run only when <c>ENCDOTNET_ENC_ROOT</c> points at a directory of them,
/// as for the benchmarks.
/// </summary>
public class S57DocumentWriterCorpusTests
{
    [Fact]
    public void RealCells_ReadBackTheSameAndWriteTheSameRecords()
    {
        var root = Environment.GetEnvironmentVariable("ENCDOTNET_ENC_ROOT");
        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
        {
            return;
        }

        var modelFailures = new List<string>();
        var recordFailures = new List<string>();
        var differingFields = new SortedDictionary<string, int>(StringComparer.Ordinal);
        int cells = 0, identicalFiles = 0, records = 0, identicalRecords = 0;

        foreach (var path in Directory.EnumerateFiles(root, "*.000", SearchOption.AllDirectories))
        {
            var source = File.ReadAllBytes(path);
            var document = S57DocumentReader.Read(source);
            var written = S57DocumentWriter.Write(document);
            var reread = S57DocumentReader.Read(written);
            cells++;

            var expected = S57DocumentFingerprint.Dump(document);
            var actual = S57DocumentFingerprint.Dump(reread);
            if (expected != actual
                || document.DataSetIdentification?.ExchangePurpose != reread.DataSetIdentification?.ExchangePurpose)
            {
                modelFailures.Add($"{Path.GetFileName(path)}: {FirstDifference(expected, actual)}");
            }

            if (source.AsSpan().SequenceEqual(written))
            {
                identicalFiles++;
            }

            var sourceRecords = Iso8211DocumentReader.Read(source).Records;
            var writtenRecords = Iso8211DocumentReader.Read(written).Records;
            if (sourceRecords.Count != writtenRecords.Count)
            {
                recordFailures.Add($"{Path.GetFileName(path)}: {sourceRecords.Count} records, wrote {writtenRecords.Count}");
                continue;
            }

            for (var i = 0; i < sourceRecords.Count; i++)
            {
                records++;
                var difference = FirstDifferingField(sourceRecords[i], writtenRecords[i]);
                if (difference is null)
                {
                    identicalRecords++;
                    continue;
                }

                differingFields[difference] = differingFields.GetValueOrDefault(difference) + 1;
                if (recordFailures.Count < 50)
                {
                    recordFailures.Add($"{Path.GetFileName(path)} record {i}: {difference}");
                }
            }
        }

        var output = TestContext.Current.TestOutputHelper;
        output?.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"{cells} cells, {identicalFiles} byte-identical; {records} records, {identicalRecords} with identical fields."));
        foreach (var (field, count) in differingFields)
        {
            output?.WriteLine($"  {field}: {count}");
        }

        foreach (var failure in recordFailures)
        {
            output?.WriteLine(failure);
        }

        Assert.Empty(modelFailures);
    }

    [Fact]
    public void RealCatalogs_WriteTheSameBytes()
    {
        var root = Environment.GetEnvironmentVariable("ENCDOTNET_ENC_ROOT");
        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
        {
            return;
        }

        var failures = new List<string>();
        foreach (var path in Directory.EnumerateFiles(root, "CATALOG.031", SearchOption.AllDirectories))
        {
            var source = File.ReadAllBytes(path);
            var catalog = S57CatalogReader.Read(source);
            var written = S57CatalogWriter.Write(catalog);
            if (!source.AsSpan().SequenceEqual(written))
            {
                failures.Add(path);
            }

            Assert.Equal(catalog.Entries, S57CatalogReader.Read(written).Entries);
        }

        Assert.Empty(failures);
    }

    // The tag of the first field whose content differs, or null when every field is the same.
    // The leader and directory follow from the fields, so they aren't compared.
    private static string? FirstDifferingField(Iso8211Record source, Iso8211Record written)
    {
        if (source.Fields.Count != written.Fields.Count)
        {
            return $"field count ({string.Join(',', source.Fields.Select(f => f.Tag))} vs {string.Join(',', written.Fields.Select(f => f.Tag))})";
        }

        for (var i = 0; i < source.Fields.Count; i++)
        {
            var a = source.Fields[i];
            var b = written.Fields[i];
            if (a.Tag != b.Tag)
            {
                return $"field order ({a.Tag} vs {b.Tag})";
            }

            if (!a.Data.Span.SequenceEqual(b.Data.Span))
            {
                return a.Tag;
            }
        }

        if (!source.Leader.Equals(written.Leader with { RecordLength = source.Leader.RecordLength, BaseAddressOfFieldArea = source.Leader.BaseAddressOfFieldArea }))
        {
            return "leader";
        }

        return null;
    }

    private static string FirstDifference(string expected, string actual)
    {
        var e = expected.Split('\n');
        var a = actual.Split('\n');
        for (var i = 0; i < Math.Min(e.Length, a.Length); i++)
        {
            if (e[i] != a[i])
            {
                return new StringBuilder().Append("line ").Append(i).Append(": '").Append(e[i]).Append("' vs '").Append(a[i]).Append('\'').ToString();
            }
        }

        return $"{e.Length} lines vs {a.Length}";
    }
}
