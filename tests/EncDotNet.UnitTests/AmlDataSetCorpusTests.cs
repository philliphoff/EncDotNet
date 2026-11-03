using System.Globalization;
using System.Text.RegularExpressions;
using EncDotNet.S57;
using EncDotNet.S57.Aml;

namespace EncDotNet.UnitTests;

/// <summary>
/// Checks real AML cells, such as the UKHO and Bundeswehr test data, against the AML model. The
/// cells are classified or licensed and are never committed, so this runs only when
/// <c>ENCDOTNET_AML_TEST_DATA</c> names a folder of them, and is skipped otherwise.
/// </summary>
public partial class AmlDataSetCorpusTests
{
    private const string TestDataVariable = "ENCDOTNET_AML_TEST_DATA";

    [Fact]
    public void RealCells_AreAmlAndEveryCodeResolvesInTheirCatalogue()
    {
        var root = Environment.GetEnvironmentVariable(TestDataVariable);
        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
        {
            Assert.Skip($"Set {TestDataVariable} to a folder of AML cells to run this test.");
        }

        var failures = new List<string>();
        int cells = 0, features = 0, values = 0;

        var paths = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(p => CellExtension().IsMatch(Path.GetExtension(p)) && !IsExchangeSetCatalogue(p))
            .Order(StringComparer.Ordinal);

        foreach (var path in paths)
        {
            var cell = Path.GetRelativePath(root, path);
            cells++;

            S57Document document;
            AmlDataSet dataSet;
            try
            {
                document = S57DocumentReader.ReadFromFile(path);
                dataSet = new AmlDataSet(document);
            }
            catch (Exception e)
            {
                failures.Add($"{cell}: can't be read as an AML cell: {e.Message}");
                continue;
            }

            if (dataSet.Product.Kind != S57ProductKind.Aml || dataSet.Product.AmlProduct is null)
            {
                failures.Add($"{cell}: product {dataSet.Product.Kind} has no AML product");
            }

            // COMT is free text in S-57. The published AML cells write AML metadata there as
            // key=value pairs, but copies re-encoded by other tools can carry a plain comment,
            // which isn't metadata. Only a comment written as pairs must parse.
            var comment = dataSet.Identification.Comment;
            if (comment is not null && comment.Contains('=', StringComparison.Ordinal) && dataSet.Metadata is null)
            {
                failures.Add($"{cell}: COMT isn't AML metadata: \"{comment}\"");
            }

            var catalogue = dataSet.Catalogue;
            var unresolvedClasses = new SortedDictionary<int, int>();
            var unresolvedAttributes = new SortedDictionary<int, int>();

            foreach (var feature in document.FeatureRecords)
            {
                features++;
                if (catalogue.GetObjectClass(feature.ObjectCode) is null)
                {
                    unresolvedClasses[(int)feature.ObjectCode] = unresolvedClasses.GetValueOrDefault((int)feature.ObjectCode) + 1;
                }

                values += Count(feature.Attributes, catalogue, unresolvedAttributes);
                values += Count(feature.NationalAttributes, catalogue, unresolvedAttributes);
            }

            foreach (var vector in document.VectorRecords)
            {
                values += Count(vector.Attributes, catalogue, unresolvedAttributes);
            }

            foreach (var (code, count) in unresolvedClasses)
            {
                failures.Add(string.Create(CultureInfo.InvariantCulture,
                    $"{cell}: OBJL {code} ({count} features) isn't in {catalogue.Name}"));
            }

            foreach (var (code, count) in unresolvedAttributes)
            {
                failures.Add(string.Create(CultureInfo.InvariantCulture,
                    $"{cell}: ATTL {code} ({count} values) isn't in {catalogue.Name}"));
            }
        }

        TestContext.Current.TestOutputHelper?.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"{cells} cells, {features} feature records, {values} attribute values, {failures.Count} failures."));

        Assert.True(cells > 0, $"{TestDataVariable} names a folder with no cells: {root}");
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private static int Count(IReadOnlyList<S57AttributeValue> attributes, S57ObjectCatalogue catalogue, SortedDictionary<int, int> unresolved)
    {
        foreach (var attribute in attributes)
        {
            if (catalogue.GetAttribute(attribute.AttributeCode) is null)
            {
                unresolved[attribute.AttributeCode] = unresolved.GetValueOrDefault(attribute.AttributeCode) + 1;
            }
        }

        return attributes.Count;
    }

    /// <summary>
    /// Gets whether a file is an exchange set catalogue (<c>CATALOG.031</c>), which has a cell-like
    /// extension but isn't a cell.
    /// </summary>
    private static bool IsExchangeSetCatalogue(string path) =>
        Path.GetFileNameWithoutExtension(path).Equals("CATALOG", StringComparison.OrdinalIgnoreCase);

    /// <summary>Matches the extension of a base cell (<c>.000</c>) or an update cell (<c>.001</c> and up).</summary>
    [GeneratedRegex(@"^\.\d{3}$")]
    private static partial Regex CellExtension();
}
