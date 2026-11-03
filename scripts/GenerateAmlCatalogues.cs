// ============================================================================
// GenerateAmlCatalogues.cs — Generates the S57ObjectCatalogue code tables for the
// NATO Additional Military Layers (AML) products from the UKHO catalogue spreadsheets.
//
// Usage (from the repository root):
//   dotnet run scripts/GenerateAmlCatalogues.cs [--inspect] <spreadsheet-dir> [<output-dir>]
//                                               [--aml10 <file>] [--aml21 <file>] [--aml30 <file>]
//
// Sources, from https://www.admiralty.co.uk/defence/additional-military-layers:
//   AML Object and Attribute Catalogue v1.1.6     (AML 1.0: 171 object classes, 385 attributes)
//   AML Object and Attribute Catalogue v2.x       (AML 2.1: 174 object classes, 387 attributes)
//   AML Feature and Attribute Catalogue v3.0.1    (AML 3.0: 258 object classes, 473 attributes)
//
// The spreadsheets are UKHO/NATO documents with no stated licence. Download them
// to a folder OUTSIDE the repository and never commit them. The script writes only
// codes, acronyms, names, classes, permitted primitives, attribute types and
// enumerated values as C# code tables; it never writes definition text.
//
// Steps to generate the tables:
//   1. Download the three spreadsheets into one folder, for example ~/aml-catalogues.
//      If a file is in the old .xls format, open it and save it as .xlsx first.
//   2. Run with --inspect to print each workbook's sheet names, header rows and the
//      sheets the script recognises. If a sheet isn't recognised, adjust the header
//      names in AmlCatalogueReader below.
//        dotnet run scripts/GenerateAmlCatalogues.cs --inspect ~/aml-catalogues
//   3. Run without --inspect to write the tables. The file name's version (v1.x,
//      v2.x, v3.x) picks the AML edition; pass --aml10/--aml21/--aml30 to choose the
//      files explicitly instead.
//        dotnet run scripts/GenerateAmlCatalogues.cs ~/aml-catalogues
//      Check the printed counts against the ones above. The script also reports codes
//      that two AML editions give different acronyms, and codes below 20000 that a
//      spreadsheet lists (those come from the IHO base catalogue, so they aren't written).
//   4. Wire each written table into src/EncDotNet.S57/S57ObjectCatalogue.cs, the same
//      way as InlandEnc, with the IHO catalogue as its base:
//        private static readonly Lazy<S57ObjectCatalogue> LazyAml10 =
//            new(() => new("AML 1.0", Iho, CreateAml10ObjectClasses(), CreateAml10Attributes()));
//      add a public Aml10 property, route S57ProductKind.Aml in For(S57ProductIdentity)
//      by Version.Major, list the catalogue in the class remarks, and add tests.
//
// Writes, in <output-dir> (default src/EncDotNet.S57), one file per edition found:
//   S57ObjectCatalogue.Aml10.g.cs   S57ObjectCatalogue.Aml21.g.cs   S57ObjectCatalogue.Aml30.g.cs
// Each holds the AML entries only (codes 20000 and up); the IHO catalogue supplies the rest,
// including C_ASSO and attribute 189 that the AML 1.0 cells use.
//
// The .xlsx reader uses only the base class library (an .xlsx is a zip of XML parts).
// tests/EncDotNet.UnitTests compiles this file with AML_CATALOGUE_TESTS defined and
// tests the reader and writer against small synthetic workbooks.
// ============================================================================

using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

#if !AML_CATALOGUE_TESTS
return AmlCatalogueGenerator.Run(args, Console.Out);
#endif

/// <summary>An AML edition the script writes a table for.</summary>
internal sealed record AmlEdition(string Table, string DisplayName, int Major, int ExpectedObjectClasses, int ExpectedAttributes)
{
    public static readonly AmlEdition Aml10 = new("Aml10", "AML 1.0", 1, 171, 385);
    public static readonly AmlEdition Aml21 = new("Aml21", "AML 2.1", 2, 174, 387);
    public static readonly AmlEdition Aml30 = new("Aml30", "AML 3.0", 3, 258, 473);

    public static IReadOnlyList<AmlEdition> All { get; } = [Aml10, Aml21, Aml30];

    /// <summary>Picks the edition from a catalogue file name's version, for example <c>v1.1.6</c>.</summary>
    public static AmlEdition? FromFileName(string fileName)
    {
        var match = Regex.Match(Path.GetFileNameWithoutExtension(fileName), @"(?i)(?:^|[^a-z0-9])v\s*(\d+)(?:[._]|$)");
        if (!match.Success)
        {
            return null;
        }

        var major = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        return All.FirstOrDefault(edition => edition.Major == major);
    }
}

internal sealed record AmlObjectClassRow(int Code, string Acronym, string Name, string Category, string Primitives);

internal sealed record AmlAttributeRow(int Code, string Acronym, string Name, string Type);

internal sealed record AmlValueRow(int Code, string Name);

/// <summary>The rows read from one AML catalogue workbook.</summary>
internal sealed class AmlCatalogueSource
{
    public List<AmlObjectClassRow> ObjectClasses { get; } = [];

    public List<AmlAttributeRow> Attributes { get; } = [];

    public Dictionary<int, List<AmlValueRow>> Values { get; } = [];

    public List<string> Messages { get; } = [];
}

/// <summary>A worksheet as rows of cell text. Missing cells and rows are empty strings.</summary>
internal sealed record XlsxSheet(string Name, IReadOnlyList<IReadOnlyList<string>> Rows);

/// <summary>Reads the cell text of an .xlsx workbook with the base class library.</summary>
internal static class XlsxReader
{
    private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace Relationships = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace PackageRelationships = "http://schemas.openxmlformats.org/package/2006/relationships";

    public static IReadOnlyList<XlsxSheet> Read(string path)
    {
        using var stream = File.OpenRead(path);
        return Read(stream);
    }

    public static IReadOnlyList<XlsxSheet> Read(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        var workbook = Load(archive, "xl/workbook.xml")
            ?? throw new InvalidDataException("The file isn't an .xlsx workbook: it has no xl/workbook.xml.");
        var targets = (Load(archive, "xl/_rels/workbook.xml.rels")?.Root?.Elements(PackageRelationships + "Relationship") ?? [])
            .ToDictionary(r => (string?)r.Attribute("Id") ?? string.Empty, r => (string?)r.Attribute("Target") ?? string.Empty);
        var sharedStrings = (Load(archive, "xl/sharedStrings.xml")?.Root?.Elements(Main + "si") ?? [])
            .Select(Text)
            .ToList();

        var sheets = new List<XlsxSheet>();
        foreach (var sheet in workbook.Root?.Element(Main + "sheets")?.Elements(Main + "sheet") ?? [])
        {
            var name = (string?)sheet.Attribute("name") ?? string.Empty;
            var id = (string?)sheet.Attribute(Relationships + "id");
            if (id is null || !targets.TryGetValue(id, out var target))
            {
                continue;
            }

            var part = target.StartsWith('/') ? target[1..] : "xl/" + target;
            var document = Load(archive, part);
            if (document is not null)
            {
                sheets.Add(new XlsxSheet(name, ReadRows(document, sharedStrings)));
            }
        }

        return sheets;
    }

    private static List<IReadOnlyList<string>> ReadRows(XDocument document, List<string> sharedStrings)
    {
        var rows = new List<IReadOnlyList<string>>();
        foreach (var row in document.Root?.Element(Main + "sheetData")?.Elements(Main + "row") ?? [])
        {
            var rowNumber = int.TryParse((string?)row.Attribute("r"), NumberStyles.None, CultureInfo.InvariantCulture, out var r)
                ? r
                : rows.Count + 1;
            while (rows.Count < rowNumber - 1)
            {
                rows.Add([]);
            }

            var cells = new List<string>();
            foreach (var cell in row.Elements(Main + "c"))
            {
                var column = ColumnIndex((string?)cell.Attribute("r")) ?? cells.Count;
                while (cells.Count < column)
                {
                    cells.Add(string.Empty);
                }

                var value = (string?)cell.Element(Main + "v") ?? string.Empty;
                var text = (string?)cell.Attribute("t") switch
                {
                    "s" => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var index)
                        && index < sharedStrings.Count ? sharedStrings[index] : string.Empty,
                    "inlineStr" => cell.Element(Main + "is") is { } inline ? Text(inline) : string.Empty,
                    _ => value,
                };

                if (column < cells.Count)
                {
                    cells[column] = text;
                }
                else
                {
                    cells.Add(text);
                }
            }

            rows.Add(cells);
        }

        return rows;
    }

    /// <summary>Gets the zero-based column of a cell reference such as <c>AB12</c>.</summary>
    private static int? ColumnIndex(string? reference)
    {
        if (string.IsNullOrEmpty(reference))
        {
            return null;
        }

        var column = 0;
        var letters = 0;
        foreach (var ch in reference)
        {
            if (ch is >= 'A' and <= 'Z')
            {
                column = (column * 26) + (ch - 'A' + 1);
                letters++;
            }
            else
            {
                break;
            }
        }

        return letters == 0 ? null : column - 1;
    }

    // Rich text keeps its text in runs; phonetic hints (rPh) aren't part of the value.
    private static string Text(XElement element) =>
        string.Concat(element.Elements(Main + "t").Concat(element.Elements(Main + "r").Elements(Main + "t")).Select(t => t.Value));

    private static XDocument? Load(ZipArchive archive, string name)
    {
        var entry = archive.GetEntry(name);
        if (entry is null)
        {
            return null;
        }

        using var stream = entry.Open();
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        return XDocument.Load(reader);
    }
}

/// <summary>
/// Finds the object class, attribute and enumerated value sheets of an AML catalogue workbook by
/// their sheet names and header rows, and reads their rows.
/// </summary>
internal static class AmlCatalogueReader
{
    private enum SheetKind
    {
        Unknown,
        ObjectClasses,
        Attributes,
        Values,
    }

    public static AmlCatalogueSource Read(IReadOnlyList<XlsxSheet> sheets)
    {
        var source = new AmlCatalogueSource();
        var objectClasses = new Dictionary<int, AmlObjectClassRow>();
        var attributes = new Dictionary<int, AmlAttributeRow>();
        var values = new List<(int? Code, string? Acronym, AmlValueRow Value)>();

        foreach (var sheet in sheets)
        {
            var (kind, headerRow, header) = Classify(sheet);
            switch (kind)
            {
                case SheetKind.ObjectClasses:
                    ReadObjectClasses(sheet, headerRow, header, objectClasses, source.Messages);
                    break;
                case SheetKind.Attributes:
                    ReadAttributes(sheet, headerRow, header, attributes, source.Messages);
                    break;
                case SheetKind.Values:
                    ReadValues(sheet, headerRow, header, values, source.Messages);
                    break;
                default:
                    source.Messages.Add($"Skipped sheet '{sheet.Name}': not an object class, attribute or enumerated value sheet.");
                    break;
            }
        }

        source.ObjectClasses.AddRange(objectClasses.Values.OrderBy(c => c.Code));
        source.Attributes.AddRange(attributes.Values.OrderBy(a => a.Code));

        var codesByAcronym = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var attribute in source.Attributes)
        {
            codesByAcronym.TryAdd(attribute.Acronym, attribute.Code);
        }

        foreach (var (code, acronym, value) in values)
        {
            var attributeCode = code ?? (acronym is not null && codesByAcronym.TryGetValue(acronym, out var c) ? c : null);
            if (attributeCode is not { } resolved)
            {
                source.Messages.Add($"Skipped value {value.Code} '{value.Name}': attribute '{acronym}' isn't in the catalogue.");
                continue;
            }

            if (!source.Values.TryGetValue(resolved, out var list))
            {
                list = [];
                source.Values[resolved] = list;
            }

            if (!list.Exists(v => v.Code == value.Code))
            {
                list.Add(value);
            }
        }

        foreach (var list in source.Values.Values)
        {
            list.Sort((x, y) => x.Code.CompareTo(y.Code));
        }

        return source;
    }

    /// <summary>Describes how the reader sees each sheet, for <c>--inspect</c>.</summary>
    public static IEnumerable<string> Describe(IReadOnlyList<XlsxSheet> sheets)
    {
        foreach (var sheet in sheets)
        {
            var (kind, headerRow, header) = Classify(sheet);
            var columns = headerRow >= 0 ? string.Join(" | ", header) : "(no header row with Code and Acronym or Meaning columns found)";
            yield return $"  '{sheet.Name}': {kind}, {sheet.Rows.Count} rows, header row {headerRow + 1}: {columns}";
        }
    }

    private static (SheetKind Kind, int HeaderRow, IReadOnlyList<string> Header) Classify(XlsxSheet sheet)
    {
        var headerRow = FindHeaderRow(sheet);
        if (headerRow < 0)
        {
            return (SheetKind.Unknown, -1, []);
        }

        var header = sheet.Rows[headerRow];
        var name = Key(sheet.Name);
        if (name.Contains("binding", StringComparison.Ordinal))
        {
            return (SheetKind.Unknown, headerRow, header);
        }

        if (MeaningColumn(header) >= 0 || ContainsAny(name, "expected", "enumerat", "listedvalue", "values", "domain"))
        {
            return (SheetKind.Values, headerRow, header);
        }

        if (name.Contains("attribute", StringComparison.Ordinal))
        {
            return (SheetKind.Attributes, headerRow, header);
        }

        if (ContainsAny(name, "object", "feature"))
        {
            return (SheetKind.ObjectClasses, headerRow, header);
        }

        if (FindContaining(header, "primitive", "geometry") >= 0)
        {
            return (SheetKind.ObjectClasses, headerRow, header);
        }

        if (TypeColumn(header) >= 0)
        {
            return (SheetKind.Attributes, headerRow, header);
        }

        return (SheetKind.Unknown, headerRow, header);
    }

    private static int FindHeaderRow(XlsxSheet sheet)
    {
        for (var i = 0; i < Math.Min(sheet.Rows.Count, 20); i++)
        {
            var row = sheet.Rows[i];
            if (CodeColumn(row) >= 0 && (FindContaining(row, "acronym") >= 0 || MeaningColumn(row) >= 0))
            {
                return i;
            }
        }

        return -1;
    }

    private static void ReadObjectClasses(
        XlsxSheet sheet,
        int headerRow,
        IReadOnlyList<string> header,
        Dictionary<int, AmlObjectClassRow> rows,
        List<string> messages)
    {
        var code = CodeColumn(header);
        var acronym = FindContaining(header, "acronym");
        var name = NameColumn(header, "objectclass", "objectclassname", "featureclass", "featurename", "feature", "object");
        var primitives = FindContaining(header, "primitive", "geometry");
        var category = Find(header, "class", "category", "objectclasstype", "type");

        foreach (var row in DataRows(sheet, headerRow))
        {
            if (!TryCode(Cell(row, code), out var value))
            {
                continue;
            }

            var entry = new AmlObjectClassRow(
                value,
                Cell(row, acronym).Trim(),
                Normalize(Cell(row, name)),
                Cell(row, category).Trim(),
                Cell(row, primitives));
            Merge(rows, value, entry, e => e.Acronym, sheet.Name, "object class", messages);
        }
    }

    private static void ReadAttributes(
        XlsxSheet sheet,
        int headerRow,
        IReadOnlyList<string> header,
        Dictionary<int, AmlAttributeRow> rows,
        List<string> messages)
    {
        var code = CodeColumn(header);
        var acronym = FindContaining(header, "acronym");
        var name = NameColumn(header, "attribute", "attributename");
        var type = TypeColumn(header);

        foreach (var row in DataRows(sheet, headerRow))
        {
            if (!TryCode(Cell(row, code), out var value))
            {
                continue;
            }

            var entry = new AmlAttributeRow(value, Cell(row, acronym).Trim(), Normalize(Cell(row, name)), Cell(row, type).Trim());
            Merge(rows, value, entry, e => e.Acronym, sheet.Name, "attribute", messages);
        }
    }

    private static void ReadValues(
        XlsxSheet sheet,
        int headerRow,
        IReadOnlyList<string> header,
        List<(int? Code, string? Acronym, AmlValueRow Value)> values,
        List<string> messages)
    {
        var attributeCode = Find(header, "attributecode", "attl", "attributeid");
        var attributeAcronym = FindContaining(header, "acronym");
        var valueCode = Find(header, "id", "value", "valueid", "valuecode", "enumerationcode", "enumeratecode", "listedvaluecode");
        var plainCode = Find(header, "code");
        if (valueCode < 0 && plainCode != attributeCode)
        {
            valueCode = plainCode;
        }
        else if (attributeCode < 0 && plainCode != valueCode)
        {
            attributeCode = plainCode;
        }

        var meaning = MeaningColumn(header) is var m and >= 0 ? m : Find(header, "name");
        if (valueCode < 0 || meaning < 0 || (attributeCode < 0 && attributeAcronym < 0))
        {
            messages.Add($"Skipped sheet '{sheet.Name}': no attribute, value and meaning columns.");
            return;
        }

        // Some layouts give the attribute once and leave it blank on the rows of its other values.
        int? currentCode = null;
        string? currentAcronym = null;
        foreach (var row in DataRows(sheet, headerRow))
        {
            int? rowCode = TryCode(Cell(row, attributeCode), out var code) ? code : null;
            var rowAcronym = Cell(row, attributeAcronym).Trim();
            if (rowCode is not null || rowAcronym.Length > 0)
            {
                currentCode = rowCode;
                currentAcronym = rowAcronym.Length > 0 ? rowAcronym : null;
            }

            var name = Normalize(Cell(row, meaning));
            if (!TryCode(Cell(row, valueCode), out var value) || name.Length == 0 || (currentCode is null && currentAcronym is null))
            {
                continue;
            }

            values.Add((currentCode, currentAcronym, new AmlValueRow(value, name)));
        }
    }

    private static void Merge<T>(
        Dictionary<int, T> rows,
        int code,
        T entry,
        Func<T, string> acronym,
        string sheet,
        string what,
        List<string> messages)
    {
        if (rows.TryGetValue(code, out var existing))
        {
            if (!string.Equals(acronym(existing), acronym(entry), StringComparison.Ordinal))
            {
                messages.Add($"Sheet '{sheet}' gives {what} {code} the acronym '{acronym(entry)}', not '{acronym(existing)}'; kept the first.");
            }

            return;
        }

        rows[code] = entry;
    }

    private static IEnumerable<IReadOnlyList<string>> DataRows(XlsxSheet sheet, int headerRow) => sheet.Rows.Skip(headerRow + 1);

    private static int CodeColumn(IReadOnlyList<string> header) =>
        Find(header, "code", "objl", "attl", "objectclasscode", "featurecode", "attributecode", "numericcode", "numericalcode");

    private static int MeaningColumn(IReadOnlyList<string> header) =>
        Find(header, "meaning", "label", "valuename", "enumerationname", "enumeratename", "listedvalue");

    private static int TypeColumn(IReadOnlyList<string> header) => Find(header, "attributetype", "type", "attrtype", "valuetype", "datatype");

    private static int NameColumn(IReadOnlyList<string> header, params string[] names)
    {
        var column = Find(header, ["name", .. names]);
        return column >= 0 ? column : FindContaining(header, "name");
    }

    private static int Find(IReadOnlyList<string> header, params string[] names)
    {
        foreach (var name in names)
        {
            for (var i = 0; i < header.Count; i++)
            {
                if (Key(header[i]) == name)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static int FindContaining(IReadOnlyList<string> header, params string[] parts)
    {
        for (var i = 0; i < header.Count; i++)
        {
            if (ContainsAny(Key(header[i]), parts))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool ContainsAny(string value, params string[] parts) =>
        parts.Any(part => value.Contains(part, StringComparison.Ordinal));

    private static string Cell(IReadOnlyList<string> row, int column) =>
        column >= 0 && column < row.Count ? row[column] : string.Empty;

    // Numeric cells can come back as "20484" or "20484.0".
    private static bool TryCode(string text, out int code)
    {
        code = 0;
        if (!double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            || number <= 0 || number > int.MaxValue || number != Math.Floor(number))
        {
            return false;
        }

        code = (int)number;
        return true;
    }

    /// <summary>Lowercase letters and digits only, so <c>Attribute Type</c> matches <c>attributetype</c>.</summary>
    private static string Key(string value) =>
        string.Concat(value.Where(char.IsLetterOrDigit)).ToLowerInvariant();

    public static string Normalize(string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}

/// <summary>Writes an AML catalogue table as a partial of <c>S57ObjectCatalogue</c>.</summary>
internal static class AmlCatalogueWriter
{
    /// <summary>The first AML code. Lower codes come from the IHO base catalogue.</summary>
    public const int FirstAmlCode = 20000;

    public static string Write(AmlEdition edition, string sourceName, AmlCatalogueSource source)
    {
        var classes = source.ObjectClasses.Where(c => c.Code >= FirstAmlCode).OrderBy(c => c.Code).ToList();
        var attributes = source.Attributes.Where(a => a.Code >= FirstAmlCode).OrderBy(a => a.Code).ToList();
        var description = $"the {edition.DisplayName} extensions (codes {FirstAmlCode} and up) to the IHO catalogue";

        var builder = new StringBuilder();
        builder.Append($$"""
            // <auto-generated>
            //   Generated by scripts/GenerateAmlCatalogues.cs from the UKHO {{sourceName}}
            //   (https://www.admiralty.co.uk/defence/additional-military-layers). Holds codes, acronyms,
            //   names, classes, primitives, attribute types and enumerated values only.
            //   Don't edit this file by hand; change the script and run it again.
            // </auto-generated>

            using static EncDotNet.S57.S57AttributeType;
            using static EncDotNet.S57.S57ObjectClassCategory;
            using static EncDotNet.S57.S57PermittedPrimitives;

            namespace EncDotNet.S57;

            public sealed partial class S57ObjectCatalogue
            {
                /// <summary>Creates the object classes of {{description}}.</summary>
                private static S57ObjectClassDefinition[] Create{{edition.Table}}ObjectClasses() =>
                [

            """);

        foreach (var c in classes)
        {
            builder.Append(CultureInfo.InvariantCulture,
                $"        new({c.Code}, {Literal(c.Acronym)}, {Literal(c.Name)}, {Category(c.Category, c.Acronym)}, {Primitives(c.Primitives)}),\n");
        }

        builder.Append($$"""
                ];

                /// <summary>Creates the attributes of {{description}}.</summary>
                private static S57AttributeDefinition[] Create{{edition.Table}}Attributes() =>
                [

            """);

        foreach (var a in attributes)
        {
            var head = $"        new({a.Code}, {Literal(a.Acronym)}, {Literal(a.Name)}, {Type(a.Type)}";
            if (source.Values.TryGetValue(a.Code, out var list) && list.Count > 0)
            {
                builder.Append(head).Append(",\n        [\n");
                foreach (var value in list)
                {
                    builder.Append(CultureInfo.InvariantCulture, $"            new({value.Code}, {Literal(value.Name)}),\n");
                }

                builder.Append("        ]),\n");
            }
            else
            {
                builder.Append(head).Append("),\n");
            }
        }

        builder.Append("""
                ];
            }

            """);

        return builder.ToString().ReplaceLineEndings("\n");
    }

    public static string Category(string value, string acronym) =>
        AmlCatalogueReader.Normalize(value).ToUpperInvariant() switch
        {
            "G" or "GEO" => "Geo",
            "M" or "META" => "Meta",
            "C" or "COLLECTION" => "Collection",
            "$" or "CARTOGRAPHIC" => "Cartographic",
            _ when acronym.StartsWith("M_", StringComparison.OrdinalIgnoreCase) => "Meta",
            _ when acronym.StartsWith("C_", StringComparison.OrdinalIgnoreCase) => "Collection",
            _ when acronym.StartsWith('$') => "Cartographic",
            _ => "Geo",
        };

    public static string Type(string value) =>
        string.Concat(value.Where(char.IsLetter)).ToUpperInvariant() switch
        {
            "E" or "ENUMERATED" or "ENUMERATION" => "Enumerated",
            "L" or "LIST" => "List",
            "F" or "FLOAT" or "REAL" => "Float",
            "I" or "INTEGER" => "Integer",
            "A" or "CODEDSTRING" or "ASCII" => "CodedString",
            "S" or "FREETEXT" or "STRING" or "TEXT" => "FreeText",
            _ => "S57AttributeType.Unknown",
        };

    public static string Primitives(string value)
    {
        var flags = new List<string>();
        foreach (var token in value.Split([';', ',', '/', ' ', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var flag = token.ToUpperInvariant() switch
            {
                "POINT" or "P" => "Point",
                "LINE" or "L" => "Line",
                "AREA" or "A" => "Area",
                _ => null,
            };

            if (flag is not null && !flags.Contains(flag))
            {
                flags.Add(flag);
            }
        }

        if (flags.Count == 0)
        {
            return "None";
        }

        // Point, Line, Area order regardless of the source order.
        flags.Sort((x, y) => Order(x).CompareTo(Order(y)));
        return string.Join(" | ", flags);

        static int Order(string flag) => flag switch { "Point" => 0, "Line" => 1, _ => 2 };
    }

    public static string Literal(string value)
    {
        var builder = new StringBuilder("\"");
        foreach (var ch in value)
        {
            builder.Append(ch switch
            {
                '"' => "\\\"",
                '\\' => "\\\\",
                _ => ch.ToString(),
            });
        }

        return builder.Append('"').ToString();
    }
}

/// <summary>The command line: finds the workbooks, reads them and writes the tables.</summary>
internal static class AmlCatalogueGenerator
{
    public static int Run(string[] args, TextWriter output)
    {
        var inspect = false;
        var positional = new List<string>();
        var explicitFiles = new Dictionary<AmlEdition, string>();
        for (var i = 0; i < args.Length; i++)
        {
            var edition = AmlEdition.All.FirstOrDefault(e => string.Equals(args[i], "--" + e.Table, StringComparison.OrdinalIgnoreCase));
            if (args[i] == "--inspect")
            {
                inspect = true;
            }
            else if (edition is not null && i + 1 < args.Length)
            {
                explicitFiles[edition] = args[++i];
            }
            else
            {
                positional.Add(args[i]);
            }
        }

        if (positional.Count == 0 && explicitFiles.Count == 0)
        {
            output.WriteLine("Usage: dotnet run scripts/GenerateAmlCatalogues.cs [--inspect] <spreadsheet-dir> [<output-dir>]");
            output.WriteLine("       [--aml10 <file>] [--aml21 <file>] [--aml30 <file>]");
            return 1;
        }

        var outputDirectory = positional.Count > 1 ? positional[1] : Path.Combine("src", "EncDotNet.S57");
        var files = new Dictionary<AmlEdition, string>(explicitFiles);
        if (positional.Count > 0)
        {
            if (!Directory.Exists(positional[0]))
            {
                output.WriteLine($"Spreadsheet folder '{positional[0]}' doesn't exist.");
                return 1;
            }

            foreach (var file in Directory.EnumerateFiles(positional[0]).Order(StringComparer.Ordinal))
            {
                var extension = Path.GetExtension(file);
                var edition = AmlEdition.FromFileName(file);
                if (string.Equals(extension, ".xls", StringComparison.OrdinalIgnoreCase))
                {
                    output.WriteLine($"Skipped {Path.GetFileName(file)}: save .xls files as .xlsx first.");
                }
                else if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase) || edition is null || explicitFiles.ContainsKey(edition))
                {
                    continue;
                }
                else if (files.TryGetValue(edition, out var other))
                {
                    output.WriteLine($"Both {Path.GetFileName(other)} and {Path.GetFileName(file)} look like {edition.DisplayName}; pass --{edition.Table.ToLowerInvariant()} <file>.");
                    return 1;
                }
                else
                {
                    files[edition] = file;
                }
            }
        }

        if (files.Count == 0)
        {
            output.WriteLine("Found no AML catalogue .xlsx files. Their names need a version such as v1.1.6, or pass --aml10/--aml21/--aml30.");
            return 1;
        }

        var sources = new Dictionary<AmlEdition, AmlCatalogueSource>();
        foreach (var edition in AmlEdition.All.Where(files.ContainsKey))
        {
            var file = files[edition];
            var sheets = XlsxReader.Read(file);
            output.WriteLine($"{edition.DisplayName}: {Path.GetFileName(file)}");
            if (inspect)
            {
                foreach (var line in AmlCatalogueReader.Describe(sheets))
                {
                    output.WriteLine(line);
                }
            }

            var source = AmlCatalogueReader.Read(sheets);
            sources[edition] = source;
            foreach (var message in source.Messages)
            {
                output.WriteLine($"  {message}");
            }

            Report(output, edition, source);

            if (!inspect)
            {
                Directory.CreateDirectory(outputDirectory);
                var path = Path.Combine(outputDirectory, $"S57ObjectCatalogue.{edition.Table}.g.cs");
                var sourceName = AmlCatalogueReader.Normalize(Path.GetFileNameWithoutExtension(file));
                File.WriteAllText(path, AmlCatalogueWriter.Write(edition, sourceName, source), new UTF8Encoding(false));
                output.WriteLine($"  Wrote {path}");
            }
        }

        foreach (var line in CompareEditions(sources))
        {
            output.WriteLine(line);
        }

        return 0;
    }

    /// <summary>Lists codes that two editions give different acronyms or types.</summary>
    public static IEnumerable<string> CompareEditions(IReadOnlyDictionary<AmlEdition, AmlCatalogueSource> sources)
    {
        var editions = AmlEdition.All.Where(sources.ContainsKey).ToList();
        for (var i = 0; i < editions.Count; i++)
        {
            for (var j = i + 1; j < editions.Count; j++)
            {
                var (a, b) = (editions[i], editions[j]);
                var classes = sources[b].ObjectClasses.ToDictionary(c => c.Code);
                foreach (var c in sources[a].ObjectClasses)
                {
                    if (classes.TryGetValue(c.Code, out var other) && !string.Equals(c.Acronym, other.Acronym, StringComparison.Ordinal))
                    {
                        yield return $"Object class {c.Code} is '{c.Acronym}' in {a.DisplayName} but '{other.Acronym}' in {b.DisplayName}.";
                    }
                }

                var attributes = sources[b].Attributes.ToDictionary(x => x.Code);
                foreach (var x in sources[a].Attributes)
                {
                    if (attributes.TryGetValue(x.Code, out var other)
                        && (!string.Equals(x.Acronym, other.Acronym, StringComparison.Ordinal)
                            || AmlCatalogueWriter.Type(x.Type) != AmlCatalogueWriter.Type(other.Type)))
                    {
                        yield return $"Attribute {x.Code} is '{x.Acronym}' ({x.Type}) in {a.DisplayName} but '{other.Acronym}' ({other.Type}) in {b.DisplayName}.";
                    }
                }
            }
        }
    }

    private static void Report(TextWriter output, AmlEdition edition, AmlCatalogueSource source)
    {
        var amlClasses = source.ObjectClasses.Count(c => c.Code >= AmlCatalogueWriter.FirstAmlCode);
        var amlAttributes = source.Attributes.Count(a => a.Code >= AmlCatalogueWriter.FirstAmlCode);
        output.WriteLine(
            $"  {source.ObjectClasses.Count} object classes ({amlClasses} AML), {source.Attributes.Count} attributes ({amlAttributes} AML), " +
            $"{source.Values.Count} enumerated attributes; expected {edition.ExpectedObjectClasses} object classes, {edition.ExpectedAttributes} attributes.");

        var ihoClasses = source.ObjectClasses.Where(c => c.Code < AmlCatalogueWriter.FirstAmlCode).Select(c => c.Acronym).ToList();
        var ihoAttributes = source.Attributes.Where(a => a.Code < AmlCatalogueWriter.FirstAmlCode).Select(a => a.Acronym).ToList();
        if (ihoClasses.Count + ihoAttributes.Count > 0)
        {
            output.WriteLine($"  Left to the IHO base catalogue: classes {string.Join(", ", ihoClasses)}; attributes {string.Join(", ", ihoAttributes)}.");
        }

        foreach (var attribute in source.Attributes.Where(a => AmlCatalogueWriter.Type(a.Type) == "S57AttributeType.Unknown"))
        {
            output.WriteLine($"  Attribute {attribute.Code} '{attribute.Acronym}' has an unrecognised type '{attribute.Type}'.");
        }
    }
}
