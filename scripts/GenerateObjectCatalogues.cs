#!/usr/bin/env dotnet run

using System.Globalization;
using System.Text;

// ============================================================================
// GenerateObjectCatalogues.cs — Generates the built-in S57ObjectCatalogue code tables.
//
// Usage (from the repository root):
//   dotnet run scripts/GenerateObjectCatalogues.cs [<gdal-s57-data-dir> | ""] [<output-dir>]
//
// Source: the S-57 object catalogue tables GDAL ships with its S-57 driver
// (ogr/ogrsf_frmts/s57/data in https://github.com/OSGeo/gdal, MIT licence):
//   s57objectclasses.csv   object classes (code, name, acronym, class, primitives)
//   s57attributes.csv      attributes (code, name, acronym, type)
//   s57expectedinput.csv   enumerated values (attribute code, ID, meaning)
// GDAL derived them from the IHO S-57 Edition 3.1 Object Catalogue. Codes
// 17000-19999 are the Inland ENC entries GDAL kept from its former
// s57objectclasses_iw.csv/s57attributes_iw.csv; GDAL has no Inland ENC
// enumerations. Codes 20000 and up are AML entries and aren't used here.
//
// Without a data directory the script downloads the files from the GDAL tag
// below. Only codes, acronyms, names, classes, primitives, attribute types and
// enumerated values are written; definition text is never included.
//
// Writes, in <output-dir> (default src/EncDotNet.S57):
//   S57ObjectCatalogue.Iho.g.cs        codes 1-16999
//   S57ObjectCatalogue.InlandEnc.g.cs  codes 17000-19999
// ============================================================================

const string GdalTag = "v3.12.0";
const string GdalDataUrl = $"https://raw.githubusercontent.com/OSGeo/gdal/{GdalTag}/ogr/ogrsf_frmts/s57/data/";

string[] sourceFiles = ["s57objectclasses.csv", "s57attributes.csv", "s57expectedinput.csv"];

var dataDirectory = args.Length > 0 && args[0].Length > 0 ? args[0] : await DownloadAsync();
var outputDirectory = args.Length > 1 ? args[1] : Path.Combine("src", "EncDotNet.S57");

var objectClasses = ReadCsv(Path.Combine(dataDirectory, "s57objectclasses.csv"))
    .Select(row => new ObjectClassRow(
        int.Parse(row["Code"], CultureInfo.InvariantCulture),
        row["Acronym"].Trim(),
        Normalize(row["ObjectClass"]),
        row["Class"].Trim(),
        row["Primitives"]))
    .Where(row => row.Code > 0)
    .ToList();

var attributes = ReadCsv(Path.Combine(dataDirectory, "s57attributes.csv"))
    .Select(row => new AttributeRow(
        int.Parse(row["Code"], CultureInfo.InvariantCulture),
        row["Acronym"].Trim(),
        Normalize(row["Attribute"]),
        row["Attributetype"].Trim()))
    .Where(row => row.Code > 0)
    .ToList();

var values = ReadCsv(Path.Combine(dataDirectory, "s57expectedinput.csv"))
    .Select(row => (
        Attribute: int.Parse(row["Code"], CultureInfo.InvariantCulture),
        Value: new ValueRow(int.Parse(row["ID"], CultureInfo.InvariantCulture), Normalize(row["Meaning"]))))
    .GroupBy(row => row.Attribute)
    .ToDictionary(group => group.Key, group => group.Select(row => row.Value).OrderBy(v => v.Code).ToList());

// Corrections to the GDAL tables.
//
// CAT_TS (188) and PUNITS (189) were added by S-57 supplements and have no
// expected input in GDAL. Their values come from S-57 Edition 3.1, Appendix A,
// Chapter 2, §2.97 and §2.164 (standards/md/S57v31AppendixACh2.md).
values.TryAdd(188, [new(1, "flood stream"), new(2, "ebb stream"), new(3, "other tidal flow")]);
values.TryAdd(189, [new(1, "metres"), new(2, "degrees of arc"), new(3, "millimeters"), new(4, "feet"), new(5, "cables")]);

// GDAL's Inland ENC table runs the acronym into one attribute name.
Rename(attributes, 17002, "Category of signal station trafficcatsit", "Category of signal station traffic");

Directory.CreateDirectory(outputDirectory);

WriteTable(
    Path.Combine(outputDirectory, "S57ObjectCatalogue.Iho.g.cs"),
    "Iho",
    "the IHO S-57 Edition 3.1 Object Catalogue",
    objectClasses.Where(c => c.Code < 17000),
    attributes.Where(a => a.Code < 17000));

WriteTable(
    Path.Combine(outputDirectory, "S57ObjectCatalogue.InlandEnc.g.cs"),
    "InlandEnc",
    "the Inland ENC extensions (codes 17000 and up) to the IHO catalogue",
    objectClasses.Where(c => c.Code is >= 17000 and < 20000),
    attributes.Where(a => a.Code is >= 17000 and < 20000));

return;

async Task<string> DownloadAsync()
{
    var directory = Directory.CreateTempSubdirectory("gdal-s57-").FullName;
    using var http = new HttpClient();
    foreach (var file in sourceFiles)
    {
        Console.WriteLine($"Downloading {GdalDataUrl}{file}");
        var bytes = await http.GetByteArrayAsync(GdalDataUrl + file);
        await File.WriteAllBytesAsync(Path.Combine(directory, file), bytes);
    }

    return directory;
}

void WriteTable(
    string path,
    string table,
    string description,
    IEnumerable<ObjectClassRow> classes,
    IEnumerable<AttributeRow> attributeRows)
{
    var classList = classes.OrderBy(c => c.Code).ToList();
    var attributeList = attributeRows.OrderBy(a => a.Code).ToList();

    var builder = new StringBuilder();
    builder.Append($$"""
        // <auto-generated>
        //   Generated by scripts/GenerateObjectCatalogues.cs from the S-57 object catalogue tables in
        //   GDAL {{GdalTag}} (https://github.com/OSGeo/gdal, ogr/ogrsf_frmts/s57/data, MIT licence).
        //   Don't edit this file by hand; change the script and run it again.
        // </auto-generated>

        using static EncDotNet.S57.S57AttributeType;
        using static EncDotNet.S57.S57ObjectClassCategory;
        using static EncDotNet.S57.S57PermittedPrimitives;

        namespace EncDotNet.S57;

        public sealed partial class S57ObjectCatalogue
        {
            /// <summary>Creates the object classes of {{description}}.</summary>
            private static S57ObjectClassDefinition[] Create{{table}}ObjectClasses() =>
            [

        """);

    foreach (var c in classList)
    {
        builder.Append(CultureInfo.InvariantCulture,
            $"        new({c.Code}, {Literal(c.Acronym)}, {Literal(c.Name)}, {Category(c.Category)}, {Primitives(c.Primitives)}),\n");
    }

    builder.Append($$"""
            ];

            /// <summary>Creates the attributes of {{description}}.</summary>
            private static S57AttributeDefinition[] Create{{table}}Attributes() =>
            [

        """);

    foreach (var a in attributeList)
    {
        var head = $"        new({a.Code}, {Literal(a.Acronym)}, {Literal(a.Name)}, {Type(a.Type)}";
        if (values.TryGetValue(a.Code, out var list) && list.Count > 0)
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

    File.WriteAllText(path, builder.ToString().ReplaceLineEndings("\n"), new UTF8Encoding(false));
    Console.WriteLine($"Wrote {path}: {classList.Count} object classes, {attributeList.Count} attributes");
}

static void Rename(List<AttributeRow> rows, int code, string from, string to)
{
    var index = rows.FindIndex(r => r.Code == code && r.Name == from);
    if (index >= 0)
    {
        rows[index] = rows[index] with { Name = to };
    }
}

static string Category(string value) => value switch
{
    "G" => "Geo",
    "M" => "Meta",
    "C" => "Collection",
    "$" => "Cartographic",
    _ => "S57ObjectClassCategory.Unknown",
};

static string Type(string value) => value switch
{
    "E" => "Enumerated",
    "L" => "List",
    "F" => "Float",
    "I" => "Integer",
    "A" => "CodedString",
    "S" => "FreeText",
    _ => "S57AttributeType.Unknown",
};

static string Primitives(string value)
{
    var flags = new List<string>();
    foreach (var token in value.Split([';', ' '], StringSplitOptions.RemoveEmptyEntries))
    {
        var flag = token switch
        {
            "Point" or "P" => "Point",
            "Line" or "L" => "Line",
            "Area" or "A" => "Area",
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

static string Normalize(string value) =>
    string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

static string Literal(string value)
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

// GDAL's files mix encodings: most are ASCII or UTF-8, but s57expectedinput.csv is Latin-1.
static string ReadText(string path)
{
    var bytes = File.ReadAllBytes(path);
    try
    {
        return new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bytes);
    }
    catch (DecoderFallbackException)
    {
        return Encoding.Latin1.GetString(bytes);
    }
}

static IEnumerable<Dictionary<string, string>> ReadCsv(string path)
{
    var lines = ReadText(path).ReplaceLineEndings("\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
    var header = ParseCsvLine(lines[0]);
    foreach (var line in lines.Skip(1))
    {
        var fields = ParseCsvLine(line);
        var row = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 0; i < header.Count; i++)
        {
            row[header[i]] = i < fields.Count ? fields[i] : string.Empty;
        }

        yield return row;
    }
}

static List<string> ParseCsvLine(string line)
{
    var fields = new List<string>();
    var field = new StringBuilder();
    var quoted = false;
    for (var i = 0; i < line.Length; i++)
    {
        var ch = line[i];
        if (quoted)
        {
            if (ch == '"' && i + 1 < line.Length && line[i + 1] == '"')
            {
                field.Append('"');
                i++;
            }
            else if (ch == '"')
            {
                quoted = false;
            }
            else
            {
                field.Append(ch);
            }
        }
        else if (ch == '"')
        {
            quoted = true;
        }
        else if (ch == ',')
        {
            fields.Add(field.ToString());
            field.Clear();
        }
        else
        {
            field.Append(ch);
        }
    }

    fields.Add(field.ToString());
    return fields;
}

record ObjectClassRow(int Code, string Acronym, string Name, string Category, string Primitives);

record AttributeRow(int Code, string Acronym, string Name, string Type);

record ValueRow(int Code, string Name);
