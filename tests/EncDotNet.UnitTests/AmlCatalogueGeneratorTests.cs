using System.IO.Compression;
using System.Security;
using System.Text;

namespace EncDotNet.UnitTests;

/// <summary>
/// Tests for the reader and writer in <c>scripts/GenerateAmlCatalogues.cs</c>, against small
/// synthetic workbooks. The codes, acronyms and names are invented; none come from the UKHO catalogues.
/// </summary>
public class AmlCatalogueGeneratorTests
{
    [Fact]
    public void XlsxReader_ReadsSharedInlineAndNumericCells_InSheetOrder()
    {
        var sheets = XlsxReader.Read(Workbook(
            ("First", [["shared", new Inline("inline"), 42, 20484.0]]),
            ("Second", [["more"]])));

        Assert.Equal(["First", "Second"], sheets.Select(s => s.Name));
        Assert.Equal(["shared", "inline", "42", "20484"], sheets[0].Rows[0]);
        Assert.Equal(["more"], sheets[1].Rows[0]);
    }

    [Fact]
    public void XlsxReader_FillsSkippedCellsAndRows()
    {
        var sheets = XlsxReader.Read(Workbook(("Sparse", [["a", null, "c"], [], [null, "b"]])));

        var rows = sheets[0].Rows;
        Assert.Equal(3, rows.Count);
        Assert.Equal(["a", "", "c"], rows[0]);
        Assert.Empty(rows[1]);
        Assert.Equal(["", "b"], rows[2]);
    }

    [Fact]
    public void XlsxReader_RejectsFileWithoutWorkbook()
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            archive.CreateEntry("readme.txt");
        }

        stream.Position = 0;
        Assert.Throws<InvalidDataException>(() => XlsxReader.Read(stream));
    }

    [Fact]
    public void Reader_ReadsObjectClasses_BelowTitleRows()
    {
        var source = ReadSyntheticCatalogue();

        Assert.Equal([1, 29001, 29002, 29003], source.ObjectClasses.Select(c => c.Code));
        var area = source.ObjectClasses[1];
        Assert.Equal(new AmlObjectClassRow(29001, "tstare", "Test area", "", "Area"), area);
        Assert.Equal("Test point", source.ObjectClasses[2].Name);
    }

    [Fact]
    public void Reader_ReadsAttributes()
    {
        var source = ReadSyntheticCatalogue();

        Assert.Equal([29100, 29101, 29102, 29103], source.Attributes.Select(a => a.Code));
        Assert.Equal(new AmlAttributeRow(29100, "tstenm", "Test enumeration", "E"), source.Attributes[0]);
        Assert.Equal("List", source.Attributes[1].Type);
    }

    [Fact]
    public void Reader_ReadsValues_ByCodeOrAcronym_CarryingTheAttributeDown()
    {
        var source = ReadSyntheticCatalogue();

        Assert.Equal([new AmlValueRow(1, "first value"), new AmlValueRow(2, "second value")], source.Values[29100]);
        Assert.Equal([new AmlValueRow(1, "list value")], source.Values[29101]);
        Assert.Equal(2, source.Values.Count);
    }

    [Fact]
    public void Reader_SkipsBindingAndUnrecognisedSheets()
    {
        var source = ReadSyntheticCatalogue();

        Assert.Contains(source.Messages, m => m.Contains("'Feature Attribute Bindings'", StringComparison.Ordinal));
        Assert.Contains(source.Messages, m => m.Contains("'Notes'", StringComparison.Ordinal));
    }

    [Fact]
    public void Reader_ReportsConflictingDuplicateCodes()
    {
        var source = AmlCatalogueReader.Read(XlsxReader.Read(Workbook(
            ("Object Classes A", [["Code", "Acronym", "Name"], [29001, "tstare", "Test area"]]),
            ("Object Classes B", [["Code", "Acronym", "Name"], [29001, "tstoth", "Test other"]]))));

        var objectClass = Assert.Single(source.ObjectClasses);
        Assert.Equal("tstare", objectClass.Acronym);
        Assert.Contains(source.Messages, m => m.Contains("29001", StringComparison.Ordinal));
    }

    [Fact]
    public void Writer_WritesAmlCodesOnly()
    {
        var code = AmlCatalogueWriter.Write(AmlEdition.Aml10, "Synthetic catalogue v1.0", ReadSyntheticCatalogue());

        Assert.Contains("private static S57ObjectClassDefinition[] CreateAml10ObjectClasses() =>", code, StringComparison.Ordinal);
        Assert.Contains("private static S57AttributeDefinition[] CreateAml10Attributes() =>", code, StringComparison.Ordinal);
        Assert.Contains("        new(29001, \"tstare\", \"Test area\", Geo, Area),\n", code, StringComparison.Ordinal);
        Assert.Contains("        new(29002, \"tstpnt\", \"Test point\", Geo, Point | Line),\n", code, StringComparison.Ordinal);
        Assert.Contains("        new(29003, \"m_tstm\", \"Test meta\", Meta, None),\n", code, StringComparison.Ordinal);
        Assert.Contains(
            "        new(29100, \"tstenm\", \"Test enumeration\", Enumerated,\n        [\n            new(1, \"first value\"),\n            new(2, \"second value\"),\n        ]),\n",
            code,
            StringComparison.Ordinal);
        Assert.Contains("        new(29102, \"tsttxt\", \"Test \\\"quoted\\\" text\", FreeText),\n", code, StringComparison.Ordinal);
        Assert.Contains("        new(29103, \"tstodd\", \"Test odd type\", S57AttributeType.Unknown),\n", code, StringComparison.Ordinal);
        Assert.DoesNotContain("TSTONE", code, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", code, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("E", "Enumerated")]
    [InlineData("Enumeration", "Enumerated")]
    [InlineData("l", "List")]
    [InlineData("Float", "Float")]
    [InlineData("I", "Integer")]
    [InlineData("A", "CodedString")]
    [InlineData("Coded string", "CodedString")]
    [InlineData("S", "FreeText")]
    [InlineData("Free text", "FreeText")]
    [InlineData("", "S57AttributeType.Unknown")]
    public void Writer_MapsAttributeType(string type, string expected) =>
        Assert.Equal(expected, AmlCatalogueWriter.Type(type));

    [Theory]
    [InlineData("Area; Point", "Point | Area")]
    [InlineData("P,L,A", "Point | Line | Area")]
    [InlineData("line / area", "Line | Area")]
    [InlineData("", "None")]
    public void Writer_MapsPrimitives(string primitives, string expected) =>
        Assert.Equal(expected, AmlCatalogueWriter.Primitives(primitives));

    [Theory]
    [InlineData("", "tstare", "Geo")]
    [InlineData("", "m_tstm", "Meta")]
    [InlineData("", "c_tsta", "Collection")]
    [InlineData("", "$tstxt", "Cartographic")]
    [InlineData("M", "tstare", "Meta")]
    [InlineData("Geo", "m_tstm", "Geo")]
    public void Writer_MapsCategory(string category, string acronym, string expected) =>
        Assert.Equal(expected, AmlCatalogueWriter.Category(category, acronym));

    [Theory]
    [InlineData("AML Object and Attribute Catalogue v1.1.6.xlsx", "Aml10")]
    [InlineData("AML_Catalogue_v2.1.xlsx", "Aml21")]
    [InlineData("AML Feature and Attribute Catalogue v3.0.1.xlsx", "Aml30")]
    [InlineData("catalogue v3.xlsx", "Aml30")]
    [InlineData("catalogue v4.0.xlsx", null)]
    [InlineData("catalogue.xlsx", null)]
    [InlineData("dev1.0.xlsx", null)]
    public void Edition_FromFileName(string fileName, string? table) =>
        Assert.Equal(table, AmlEdition.FromFileName(fileName)?.Table);

    [Fact]
    public void CompareEditions_ReportsCodesWithDifferentMeanings()
    {
        var older = new AmlCatalogueSource();
        older.ObjectClasses.Add(new(29001, "tstare", "Test area", "", "Area"));
        older.Attributes.Add(new(29100, "tstenm", "Test enumeration", "E"));
        older.Attributes.Add(new(29101, "tstlst", "Test list", "L"));
        var newer = new AmlCatalogueSource();
        newer.ObjectClasses.Add(new(29001, "tstoth", "Test other", "", "Area"));
        newer.Attributes.Add(new(29100, "tstenm", "Test enumeration", "Enumerated"));
        newer.Attributes.Add(new(29101, "tstlst", "Test list", "E"));

        var lines = AmlCatalogueGenerator.CompareEditions(new Dictionary<AmlEdition, AmlCatalogueSource>
        {
            [AmlEdition.Aml10] = older,
            [AmlEdition.Aml30] = newer,
        }).ToList();

        Assert.Equal(2, lines.Count);
        Assert.Contains("Object class 29001", lines[0], StringComparison.Ordinal);
        Assert.Contains("Attribute 29101", lines[1], StringComparison.Ordinal);
    }

    [Fact]
    public void Run_WritesOneTablePerEditionFound()
    {
        var input = Directory.CreateTempSubdirectory("aml-input-");
        var output = Directory.CreateTempSubdirectory("aml-output-");
        try
        {
            File.WriteAllBytes(Path.Combine(input.FullName, "Synthetic catalogue v1.0.xlsx"), SyntheticCatalogue().ToArray());
            File.WriteAllBytes(Path.Combine(input.FullName, "Synthetic catalogue v3.0.1.xlsx"), SyntheticCatalogue().ToArray());
            File.WriteAllText(Path.Combine(input.FullName, "Synthetic catalogue v2.1.xls"), "not converted");
            var log = new StringWriter();

            var exitCode = AmlCatalogueGenerator.Run([input.FullName, output.FullName], log);

            Assert.Equal(0, exitCode);
            Assert.Equal(
                ["S57ObjectCatalogue.Aml10.g.cs", "S57ObjectCatalogue.Aml30.g.cs"],
                Directory.GetFiles(output.FullName).Select(Path.GetFileName).Order(StringComparer.Ordinal));
            Assert.Contains("CreateAml30ObjectClasses", File.ReadAllText(Path.Combine(output.FullName, "S57ObjectCatalogue.Aml30.g.cs")), StringComparison.Ordinal);
            Assert.Contains("save .xls files as .xlsx", log.ToString(), StringComparison.Ordinal);
            Assert.Contains("4 object classes (3 AML), 4 attributes (4 AML)", log.ToString(), StringComparison.Ordinal);
            Assert.Contains("Left to the IHO base catalogue: classes TSTONE", log.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            input.Delete(recursive: true);
            output.Delete(recursive: true);
        }
    }

    [Fact]
    public void Run_Inspect_WritesNothing()
    {
        var input = Directory.CreateTempSubdirectory("aml-input-");
        var output = Path.Combine(input.FullName, "out");
        try
        {
            File.WriteAllBytes(Path.Combine(input.FullName, "Synthetic catalogue v2.1.xlsx"), SyntheticCatalogue().ToArray());
            var log = new StringWriter();

            var exitCode = AmlCatalogueGenerator.Run(["--inspect", input.FullName, output], log);

            Assert.Equal(0, exitCode);
            Assert.False(Directory.Exists(output));
            Assert.Contains("'Object Classes': ObjectClasses", log.ToString(), StringComparison.Ordinal);
            Assert.Contains("'Expected Input': Values", log.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            input.Delete(recursive: true);
        }
    }

    [Fact]
    public void Run_WithoutInputs_Fails()
    {
        Assert.Equal(1, AmlCatalogueGenerator.Run([], TextWriter.Null));
        Assert.Equal(1, AmlCatalogueGenerator.Run([Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())], TextWriter.Null));
    }

    private static AmlCatalogueSource ReadSyntheticCatalogue() =>
        AmlCatalogueReader.Read(XlsxReader.Read(SyntheticCatalogue()));

    /// <summary>A workbook laid out the way a catalogue might be, with invented entries.</summary>
    private static MemoryStream SyntheticCatalogue() => Workbook(
        ("Object Classes",
        [
            ["Synthetic object catalogue"],
            [],
            ["Code", "Acronym", "Object Class", "Primitives"],
            [1, "TSTONE", "Test one", "Point"],
            [29001, "tstare", new Inline("Test  area"), "Area"],
            [29002, "tstpnt", "Test point", "Point; Line"],
            [29003, "m_tstm", "Test meta", null],
            [null, null, "Footnote without a code"],
        ]),
        ("Attributes",
        [
            ["Code", "Acronym", "Attribute", "Attribute Type"],
            [29100, "tstenm", "Test enumeration", "E"],
            [29101, "tstlst", "Test list", "List"],
            [29102.0, "tsttxt", "Test \"quoted\" text", "S"],
            [29103, "tstodd", "Test odd type", "Q"],
        ]),
        ("Expected Input",
        [
            ["Attribute Code", "Acronym", "ID", "Meaning"],
            [29100, "tstenm", 1, "first value"],
            [null, null, 2, "second value"],
            [null, "tstlst", 1, "list value"],
            [null, "tstnone", 1, "unknown attribute"],
        ]),
        ("Feature Attribute Bindings",
        [
            ["Code", "Acronym", "Attribute Acronym"],
            [29001, "tstare", "tstenm"],
        ]),
        ("Notes", [["Synthetic notes"]]));

    private sealed record Inline(string Text);

    /// <summary>
    /// Builds a minimal .xlsx: strings go to the shared string table, <see cref="Inline"/> cells
    /// are inline strings, numbers are numeric cells and <c>null</c> cells are left out.
    /// </summary>
    private static MemoryStream Workbook(params (string Name, object?[][] Rows)[] sheets)
    {
        const string main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        const string rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        var shared = new List<string>();
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var workbook = new StringBuilder($"<workbook xmlns=\"{main}\" xmlns:r=\"{rel}\"><sheets>");
            var rels = new StringBuilder("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
            for (var s = 0; s < sheets.Length; s++)
            {
                workbook.Append($"<sheet name=\"{Escape(sheets[s].Name)}\" sheetId=\"{s + 1}\" r:id=\"rId{s + 1}\"/>");
                rels.Append($"<Relationship Id=\"rId{s + 1}\" Type=\"{rel}/worksheet\" Target=\"worksheets/sheet{s + 1}.xml\"/>");

                var sheet = new StringBuilder($"<worksheet xmlns=\"{main}\"><sheetData>");
                for (var r = 0; r < sheets[s].Rows.Length; r++)
                {
                    var row = sheets[s].Rows[r];
                    if (row.Length == 0)
                    {
                        continue;
                    }

                    sheet.Append($"<row r=\"{r + 1}\">");
                    for (var c = 0; c < row.Length; c++)
                    {
                        var reference = $"{(char)('A' + c)}{r + 1}";
                        switch (row[c])
                        {
                            case null:
                                break;
                            case Inline inline:
                                sheet.Append($"<c r=\"{reference}\" t=\"inlineStr\"><is><t>{Escape(inline.Text)}</t></is></c>");
                                break;
                            case string text:
                                shared.Add(text);
                                sheet.Append($"<c r=\"{reference}\" t=\"s\"><v>{shared.Count - 1}</v></c>");
                                break;
                            case IFormattable number:
                                sheet.Append($"<c r=\"{reference}\"><v>{number.ToString(null, System.Globalization.CultureInfo.InvariantCulture)}</v></c>");
                                break;
                        }
                    }

                    sheet.Append("</row>");
                }

                Add(archive, $"xl/worksheets/sheet{s + 1}.xml", sheet.Append("</sheetData></worksheet>").ToString());
            }

            Add(archive, "xl/workbook.xml", workbook.Append("</sheets></workbook>").ToString());
            Add(archive, "xl/_rels/workbook.xml.rels", rels.Append("</Relationships>").ToString());

            // One shared string as rich text runs, as Excel writes formatted text.
            var strings = new StringBuilder($"<sst xmlns=\"{main}\">");
            foreach (var text in shared)
            {
                strings.Append(text == "Test point"
                    ? "<si><r><t>Test </t></r><r><t>point</t></r><rPh><t>ignored</t></rPh></si>"
                    : $"<si><t>{Escape(text)}</t></si>");
            }

            Add(archive, "xl/sharedStrings.xml", strings.Append("</sst>").ToString());
        }

        stream.Position = 0;
        return stream;

        static void Add(ZipArchive archive, string name, string content)
        {
            using var writer = new StreamWriter(archive.CreateEntry(name).Open(), new UTF8Encoding(false));
            writer.Write(content);
        }

        static string Escape(string text) => SecurityElement.Escape(text);
    }
}
