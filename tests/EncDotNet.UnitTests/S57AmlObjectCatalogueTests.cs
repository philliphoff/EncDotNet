using EncDotNet.S57;
using EncDotNet.S57.Aml;

namespace EncDotNet.UnitTests;

/// <summary>
/// Unit tests for the built-in AML catalogues of <see cref="S57ObjectCatalogue"/>.
/// </summary>
public class S57AmlObjectCatalogueTests
{
    public static TheoryData<string, int, int> AmlEntryCounts => new()
    {
        { "AML 1.0", 65, 232 },
        { "AML 2.1", 67, 234 },
        { "AML 3.0", 87, 294 },
    };

    [Theory]
    [MemberData(nameof(AmlEntryCounts))]
    public void Aml_AddsItsEntriesToTheIhoCatalogue(string name, int objectClasses, int attributes)
    {
        var catalogue = Catalogue(name);

        Assert.Equal(name, catalogue.Name);
        Assert.Equal(objectClasses, catalogue.ObjectClasses.Count(c => c.Code >= 20000));
        Assert.Equal(attributes, catalogue.Attributes.Count(a => a.Code >= 20000));
        Assert.Equal(S57ObjectCatalogue.Iho.ObjectClasses.Count + objectClasses, catalogue.ObjectClasses.Count);
        Assert.Equal(S57ObjectCatalogue.Iho.Attributes.Count + attributes, catalogue.Attributes.Count);
    }

    [Theory]
    [InlineData("AML 1.0")]
    [InlineData("AML 2.1")]
    [InlineData("AML 3.0")]
    public void Aml_KeepsIhoEntries(string name)
    {
        var catalogue = Catalogue(name);

        Assert.Same(S57ObjectCatalogue.Iho.GetObjectClass(S57ObjectCode.C_ASSO), catalogue.GetObjectClass(S57ObjectCode.C_ASSO));
        Assert.Same(S57ObjectCatalogue.Iho.GetObjectClass("M_COVR"), catalogue.GetObjectClass(302));
        Assert.Equal("PUNITS", catalogue.GetAttribute(189)?.Acronym);
        Assert.Same(S57ObjectCatalogue.Iho.GetAttribute("COLOUR"), catalogue.GetAttribute(75));
        Assert.Null(catalogue.GetObjectClass(17000));
    }

    [Theory]
    [InlineData("AML 1.0")]
    [InlineData("AML 2.1")]
    [InlineData("AML 3.0")]
    public void Aml_SharedClassesAndAttributes_KeepTheirAcronyms(string name)
    {
        var catalogue = Catalogue(name);

        Assert.Equal("atsctl", catalogue.GetObjectClass(20484)?.Acronym);
        Assert.Equal("ATS Route Centreline", catalogue.GetObjectClass("atsctl")?.Name);
        Assert.Equal(S57ObjectClassCategory.Meta, catalogue.GetObjectClass("m_conf")?.Category);

        var databa = catalogue.GetAttribute(20484);
        Assert.NotNull(databa);
        Assert.Equal("databa", databa.Acronym);
        Assert.Equal("Abandonment Date", databa.Name);
        Assert.Equal(S57AttributeType.CodedString, databa.Type);

        var arecat = catalogue.GetAttribute("arecat");
        Assert.NotNull(arecat);
        Assert.Equal(20687, arecat.Code);
        Assert.Equal(S57AttributeType.Enumerated, arecat.Type);
        Assert.Equal("Solid Red (SR)", arecat.GetValue(501)?.Name);
        Assert.Equal("Pecked Red (PR)", arecat.GetValue(502)?.Name);
    }

    [Fact]
    public void Aml10_HasItsOwnClassesAndPrimitives()
    {
        var catalogue = S57ObjectCatalogue.Aml10;

        var bottomFeature = catalogue.GetObjectClass("botmft");
        Assert.NotNull(bottomFeature);
        Assert.Equal(20491, bottomFeature.Code);
        Assert.Equal(S57PermittedPrimitives.Point | S57PermittedPrimitives.Line | S57PermittedPrimitives.Area, bottomFeature.Primitives);

        Assert.Equal(S57AttributeType.Integer, catalogue.GetAttribute("quarad")?.Type);
        Assert.Equal(S57AttributeType.Enumerated, catalogue.GetAttribute("mnimnc")?.Type);
        Assert.Equal("Gravelly Muddy Sand (Folk)", catalogue.GetAttribute("natsed")?.GetValue(544)?.Name);
        Assert.Equal("Ice Attribute Concentration Total", catalogue.GetAttribute(20696)?.Name);
    }

    [Fact]
    public void Aml10_LeavesOutObsoleteCodes()
    {
        Assert.Null(S57ObjectCatalogue.Aml10.GetAttribute(20508));
        Assert.Equal("catrep", S57ObjectCatalogue.Aml21.GetAttribute(20508)?.Acronym);
        Assert.Equal("catrep", S57ObjectCatalogue.Aml30.GetAttribute(20508)?.Acronym);
    }

    [Fact]
    public void Aml21_ChangesSomeTypesAndValues()
    {
        var catalogue = S57ObjectCatalogue.Aml21;

        Assert.Equal(S57AttributeType.Enumerated, catalogue.GetAttribute("quarad")?.Type);
        Assert.Equal(S57AttributeType.List, catalogue.GetAttribute("mnimnc")?.Type);
        Assert.Equal("target", catalogue.GetAttribute("typewk")?.GetValue(608)?.Name);
        Assert.Equal("Category of completeness", catalogue.GetAttribute(22484)?.Name);
    }

    [Fact]
    public void Aml30_HasItsOwnClassesAndAttributes()
    {
        var catalogue = S57ObjectCatalogue.Aml30;

        var boundary = catalogue.GetObjectClass(40101);
        Assert.NotNull(boundary);
        Assert.Equal("boundg", boundary.Acronym);
        Assert.Equal("General Boundary Line", boundary.Name);
        Assert.Equal(S57ObjectClassCategory.Geo, boundary.Category);
        Assert.Equal(S57PermittedPrimitives.None, boundary.Primitives);

        Assert.Equal(30309, catalogue.GetObjectClass("ICEBRG")?.Code);
        Assert.Equal("catbdg", catalogue.GetAttribute(40101)?.Acronym);
        Assert.Equal(S57AttributeType.List, catalogue.GetAttribute("catbdg")?.Type);
        Assert.Equal("Crystalline Rock", catalogue.GetAttribute("natsed")?.GetValue(544)?.Name);

        Assert.Null(S57ObjectCatalogue.Aml21.GetObjectClass(40101));
        Assert.Null(S57ObjectCatalogue.Aml10.GetObjectClass("boundg"));
    }

    [Theory]
    [InlineData("AML 1.0")]
    [InlineData("AML 2.1")]
    [InlineData("AML 3.0")]
    public void Aml_EnumeratedAttributes_HaveNamedValues(string name)
    {
        var values = Catalogue(name).Attributes.Where(a => a.Code >= 20000).SelectMany(a => a.Values).ToList();

        Assert.NotEmpty(values);
        Assert.DoesNotContain(values, v => string.IsNullOrWhiteSpace(v.Name));
    }

    [Theory]
    [InlineData(AmlProduct.MFF, "1.0", "AML 1.0")]
    [InlineData(AmlProduct.RAL, "1.1", "AML 1.0")]
    [InlineData(AmlProduct.LBO, "2.0", "AML 2.1")]
    [InlineData(AmlProduct.CLB, "2.1", "AML 2.1")]
    [InlineData(AmlProduct.Integrated, "3.0", "AML 3.0")]
    [InlineData(AmlProduct.ESB, "3", "AML 3.0")]
    [InlineData(AmlProduct.SBO, "", "AML 2.1")]
    [InlineData(AmlProduct.SBO, "4.0", "AML 2.1")]
    [InlineData(AmlProduct.Integrated, "", "AML 3.0")]
    [InlineData(AmlProduct.Integrated, "draft", "AML 3.0")]
    public void For_ChoosesAmlCatalogueFromEdition(AmlProduct product, string edition, string expected)
    {
        var identification = new S57DataSetIdentification
        {
            ProductSpecification = (int)product,
            ProductSpecificationEdition = edition,
        };

        Assert.Equal(expected, S57ObjectCatalogue.For(identification).Name);
    }

    [Fact]
    public void For_ReturnsSharedAmlInstances()
    {
        Assert.Same(S57ObjectCatalogue.Aml10, S57ObjectCatalogue.For(Product("1.0")));
        Assert.Same(S57ObjectCatalogue.Aml21, S57ObjectCatalogue.For(Product("2.1")));
        Assert.Same(S57ObjectCatalogue.Aml30, S57ObjectCatalogue.For(Product("3.0")));

        static S57ProductIdentity Product(string edition) =>
            new() { Kind = S57ProductKind.Aml, AmlProduct = AmlProduct.MFF, Edition = edition, Version = Version.Parse(edition) };
    }

    [Theory]
    [InlineData(AmlProduct.RAL, "1.0", 20484, 20687)]
    [InlineData(AmlProduct.LBO, "2.1", 20491, 20508)]
    [InlineData(AmlProduct.Integrated, "3.0", 40101, 40101)]
    public void SyntheticAmlCell_CodesResolveInItsCatalogue(AmlProduct product, string edition, int objectCode, int attributeCode)
    {
        var cell = AmlCell(product, edition, objectCode, attributeCode);

        var document = S57DocumentReader.Read(S57DocumentWriter.Write(cell));
        var catalogue = document.GetObjectCatalogue();

        Assert.Equal(S57ProductKind.Aml, document.DataSetIdentification?.Product.Kind);
        Assert.Same(S57ObjectCatalogue.For(document.DataSetIdentification!), catalogue);
        Assert.StartsWith("AML ", catalogue.Name, StringComparison.Ordinal);
        foreach (var feature in document.FeatureRecords)
        {
            Assert.True(catalogue.TryGetObjectClass((int)feature.ObjectCode, out _), $"Object class {(int)feature.ObjectCode} doesn't resolve.");
            foreach (var attribute in feature.Attributes)
            {
                Assert.True(catalogue.TryGetAttribute(attribute.AttributeCode, out _), $"Attribute {attribute.AttributeCode} doesn't resolve.");
            }
        }

        Assert.Contains(document.FeatureRecords, f => (int)f.ObjectCode == objectCode);
        Assert.Contains(document.FeatureRecords, f => f.ObjectCode == S57ObjectCode.C_ASSO);
        Assert.Null(S57ObjectCatalogue.Iho.GetObjectClass(objectCode));
    }

    private static S57ObjectCatalogue Catalogue(string name) => name switch
    {
        "AML 1.0" => S57ObjectCatalogue.Aml10,
        "AML 2.1" => S57ObjectCatalogue.Aml21,
        "AML 3.0" => S57ObjectCatalogue.Aml30,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, null),
    };

    /// <summary>
    /// The writer test cell recast as an AML data set: an AML feature with an AML attribute, a
    /// <c>C_ASSO</c> collection that associates it with the buoy, and attribute 189 on the coverage.
    /// </summary>
    private static S57Document AmlCell(AmlProduct product, string edition, int objectCode, int attributeCode)
    {
        var cell = S57WriterTestCells.SmallCell();
        var features = cell.FeatureRecords.ToList();
        features[0] = features[0] with { Attributes = [.. features[0].Attributes, new S57AttributeValue(189, "1")] }; // PUNITS
        features.Add(new S57FeatureRecord
        {
            RecordName = Feature(6),
            Primitive = S57GeometricPrimitive.Point,
            Group = 2,
            ObjectCode = (S57ObjectCode)objectCode,
            RecordVersion = 1,
            UpdateInstruction = S57UpdateInstruction.Insert,
            Attributes = [new S57AttributeValue(attributeCode, "1")],
            SpatialPointers = features[1].SpatialPointers,
        });
        features.Add(new S57FeatureRecord
        {
            RecordName = Feature(7),
            Primitive = S57GeometricPrimitive.None,
            Group = 2,
            ObjectCode = S57ObjectCode.C_ASSO,
            RecordVersion = 1,
            UpdateInstruction = S57UpdateInstruction.Insert,
            FeaturePointers =
            [
                new S57FeaturePointer { Name = new S57LongName(550, 6, 1), Relationship = S57RelationshipIndicator.Peer, Comment = "" },
                new S57FeaturePointer { Name = new S57LongName(550, 2, 1), Relationship = S57RelationshipIndicator.Peer, Comment = "" },
            ],
        });

        return cell with
        {
            DataSetIdentification = cell.DataSetIdentification! with
            {
                ProductSpecification = (int)product,
                ProductSpecificationEdition = edition,
                ApplicationProfile = 0,
            },
            FeatureRecords = features,
        };
    }

    private static S57RecordName Feature(int rcid) => new()
    {
        RecordNameCode = S57RecordNameCodes.Feature,
        RecordId = rcid,
        AgencyCode = 550,
        FeatureId = rcid,
        FeatureSubdivision = 1,
    };
}
