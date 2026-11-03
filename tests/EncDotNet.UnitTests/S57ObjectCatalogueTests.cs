using EncDotNet.S57;

namespace EncDotNet.UnitTests;

/// <summary>
/// Unit tests for <see cref="S57ObjectCatalogue"/> and its built-in catalogues.
/// </summary>
public class S57ObjectCatalogueTests
{
    public static TheoryData<S57ObjectCode> ObjectCodes => new(Enum.GetValues<S57ObjectCode>());

    [Theory]
    [MemberData(nameof(ObjectCodes))]
    public void Iho_ResolvesEveryObjectCode_WithMatchingAcronym(S57ObjectCode code)
    {
        var objectClass = S57ObjectCatalogue.Iho.GetObjectClass(code);

        Assert.NotNull(objectClass);
        Assert.Equal((int)code, objectClass.Code);
        Assert.Equal(code.ToString(), objectClass.Acronym);
        Assert.False(string.IsNullOrWhiteSpace(objectClass.Name));
        Assert.Same(objectClass, S57ObjectCatalogue.Iho.GetObjectClass(objectClass.Acronym));
    }

    [Fact]
    public void Iho_ObjectClass_HasNameCategoryAndPrimitives()
    {
        var lights = S57ObjectCatalogue.Iho.GetObjectClass("LIGHTS");

        Assert.NotNull(lights);
        Assert.Equal(75, lights.Code);
        Assert.Equal("Light", lights.Name);
        Assert.Equal(S57ObjectClassCategory.Geo, lights.Category);
        Assert.Equal(S57PermittedPrimitives.Point, lights.Primitives);

        var depare = S57ObjectCatalogue.Iho.GetObjectClass(S57ObjectCode.DEPARE);
        Assert.NotNull(depare);
        Assert.Equal(S57PermittedPrimitives.Line | S57PermittedPrimitives.Area, depare.Primitives);
    }

    [Theory]
    [InlineData("M_COVR", S57ObjectClassCategory.Meta)]
    [InlineData("C_AGGR", S57ObjectClassCategory.Collection)]
    [InlineData("$TEXTS", S57ObjectClassCategory.Cartographic)]
    [InlineData("ARCSLN", S57ObjectClassCategory.Geo)]
    public void Iho_ObjectClass_HasCategory(string acronym, S57ObjectClassCategory category)
    {
        Assert.True(S57ObjectCatalogue.Iho.TryGetObjectClass(acronym, out var objectClass));
        Assert.Equal(category, objectClass.Category);
    }

    [Fact]
    public void Iho_CollectionObject_HasNoPrimitives()
    {
        var objectClass = S57ObjectCatalogue.Iho.GetObjectClass("C_ASSO");

        Assert.NotNull(objectClass);
        Assert.Equal(401, objectClass.Code);
        Assert.Equal(S57PermittedPrimitives.None, objectClass.Primitives);
    }

    [Fact]
    public void Iho_Colour_IsListWithColours()
    {
        var colour = S57ObjectCatalogue.Iho.GetAttribute("COLOUR");

        Assert.NotNull(colour);
        Assert.Equal(75, colour.Code);
        Assert.Equal("Colour", colour.Name);
        Assert.Equal(S57AttributeType.List, colour.Type);
        Assert.Equal(13, colour.Values.Count);
        Assert.Equal(new S57EnumeratedValue(1, "white"), colour.Values[0]);
        Assert.Equal("red", colour.GetValue(3)?.Name);
        Assert.Equal("pink", colour.GetValue(13)?.Name);
        Assert.Null(colour.GetValue(14));
    }

    [Fact]
    public void Iho_Catlam_IsEnumeratedWithLateralMarkCategories()
    {
        Assert.True(S57ObjectCatalogue.Iho.TryGetAttribute(36, out var catlam));

        Assert.Equal("CATLAM", catlam.Acronym);
        Assert.Equal(S57AttributeType.Enumerated, catlam.Type);
        Assert.True(catlam.TryGetValue(1, out var port));
        Assert.Equal("port-hand lateral mark", port.Name);
        Assert.Equal("starboard-hand lateral mark", catlam.GetValue(2)?.Name);
        Assert.False(catlam.TryGetValue(99, out _));
    }

    [Theory]
    [InlineData("OBJNAM", 116, S57AttributeType.FreeText)]
    [InlineData("NOBJNM", 301, S57AttributeType.FreeText)]
    [InlineData("VALSOU", 179, S57AttributeType.Float)]
    [InlineData("SCAMIN", 133, S57AttributeType.Integer)]
    [InlineData("AGENCY", 1, S57AttributeType.CodedString)]
    [InlineData("BOYSHP", 4, S57AttributeType.Enumerated)]
    [InlineData("QUAPOS", 402, S57AttributeType.Enumerated)]
    public void Iho_Attribute_HasCodeAndType(string acronym, int code, S57AttributeType type)
    {
        var attribute = S57ObjectCatalogue.Iho.GetAttribute(acronym);

        Assert.NotNull(attribute);
        Assert.Equal(code, attribute.Code);
        Assert.Equal(type, attribute.Type);
        Assert.Same(attribute, S57ObjectCatalogue.Iho.GetAttribute(code));
    }

    [Fact]
    public void Iho_EnumeratedAttributes_HaveValues()
    {
        var missing = S57ObjectCatalogue.Iho.Attributes
            .Where(a => a.Type is S57AttributeType.Enumerated or S57AttributeType.List && a.Values.Count == 0)
            .Select(a => a.Acronym);

        Assert.Empty(missing);
    }

    [Fact]
    public void Iho_SupplementAttributes_HaveValues()
    {
        Assert.Equal("ebb stream", S57ObjectCatalogue.Iho.GetAttribute("CAT_TS")?.GetValue(2)?.Name);
        Assert.Equal("cables", S57ObjectCatalogue.Iho.GetAttribute("PUNITS")?.GetValue(5)?.Name);
    }

    [Fact]
    public void Iho_ValueNames_KeepNonAsciiCharacters()
    {
        // Mixed-encoding source data: s57expectedinput.csv is Latin-1.
        var names = S57ObjectCatalogue.Iho.Attributes.SelectMany(a => a.Values).Select(v => v.Name);

        Assert.Contains("moiré effect", names);
        Assert.DoesNotContain(names, n => n.Contains('�'));
    }

    [Fact]
    public void Catalogues_HaveUniqueAcronymsAndCodes()
    {
        foreach (var catalogue in new[] { S57ObjectCatalogue.Iho, S57ObjectCatalogue.InlandEnc })
        {
            AssertUnique(catalogue.ObjectClasses.Select(c => c.Acronym));
            AssertUnique(catalogue.ObjectClasses.Select(c => c.Code));
            AssertUnique(catalogue.Attributes.Select(a => a.Acronym));
            AssertUnique(catalogue.Attributes.Select(a => a.Code));
        }

        static void AssertUnique<T>(IEnumerable<T> items)
        {
            var duplicates = items.GroupBy(i => i).Where(g => g.Count() > 1).Select(g => g.Key);
            Assert.Empty(duplicates);
        }
    }

    [Fact]
    public void Iho_UnknownCodesAndAcronyms_ReturnNullOrFalse()
    {
        var catalogue = S57ObjectCatalogue.Iho;

        Assert.Null(catalogue.GetObjectClass(0));
        Assert.Null(catalogue.GetObjectClass(17000));
        Assert.Null(catalogue.GetObjectClass(20484));
        Assert.Null(catalogue.GetObjectClass("NOSUCH"));
        Assert.False(catalogue.TryGetObjectClass(99999, out var objectClass));
        Assert.Null(objectClass);
        Assert.False(catalogue.TryGetObjectClass("achbrt", out _));

        Assert.Null(catalogue.GetAttribute(0));
        Assert.Null(catalogue.GetAttribute(17000));
        Assert.Null(catalogue.GetAttribute("NOSUCH"));
        Assert.False(catalogue.TryGetAttribute(99999, out var attribute));
        Assert.Null(attribute);
        Assert.False(catalogue.TryGetAttribute("catach", out _));
    }

    [Fact]
    public void AcronymLookup_IsCaseSensitive()
    {
        Assert.Null(S57ObjectCatalogue.Iho.GetObjectClass("lights"));
        Assert.Null(S57ObjectCatalogue.Iho.GetAttribute("colour"));

        Assert.Equal(3, S57ObjectCatalogue.InlandEnc.GetObjectClass("ACHBRT")?.Code);
        Assert.Equal(17000, S57ObjectCatalogue.InlandEnc.GetObjectClass("achbrt")?.Code);
    }

    [Fact]
    public void AcronymLookup_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => S57ObjectCatalogue.Iho.GetObjectClass((string)null!));
        Assert.Throws<ArgumentNullException>(() => S57ObjectCatalogue.Iho.GetAttribute((string)null!));
    }

    [Fact]
    public void InlandEnc_IncludesIhoAndInlandEntries()
    {
        var catalogue = S57ObjectCatalogue.InlandEnc;

        Assert.Same(S57ObjectCatalogue.Iho.GetObjectClass(S57ObjectCode.LIGHTS), catalogue.GetObjectClass(S57ObjectCode.LIGHTS));
        Assert.Same(S57ObjectCatalogue.Iho.GetAttribute("COLOUR"), catalogue.GetAttribute("COLOUR"));
        Assert.Equal(S57ObjectCatalogue.Iho.ObjectClasses.Count + 35, catalogue.ObjectClasses.Count);
        Assert.Equal(S57ObjectCatalogue.Iho.Attributes.Count + 37, catalogue.Attributes.Count);

        var notice = catalogue.GetObjectClass(17050);
        Assert.NotNull(notice);
        Assert.Equal("notmrk", notice.Acronym);
        Assert.Equal(S57ObjectClassCategory.Geo, notice.Category);

        var catsit = catalogue.GetAttribute("catsit");
        Assert.NotNull(catsit);
        Assert.Equal(17002, catsit.Code);
        Assert.Equal("Category of signal station traffic", catsit.Name);
        Assert.Equal(S57AttributeType.List, catsit.Type);
        Assert.Equal(S57AttributeType.FreeText, catalogue.GetAttribute("unlocd")?.Type);
    }

    [Theory]
    [InlineData(0, "IHO S-57 Edition 3.1")]
    [InlineData(1, "IHO S-57 Edition 3.1")]
    [InlineData(10, "Inland ENC")]
    [InlineData(51, "IHO S-57 Edition 3.1")]
    public void For_ChoosesCatalogueFromProductSpecification(int productSpecification, string expected)
    {
        var identification = new S57DataSetIdentification { ProductSpecification = productSpecification };

        Assert.Equal(expected, S57ObjectCatalogue.For(identification).Name);
    }

    [Fact]
    public void For_ReturnsSharedInstances()
    {
        Assert.Same(S57ObjectCatalogue.Iho, S57ObjectCatalogue.For(new S57DataSetIdentification { ProductSpecification = 1 }));
        Assert.Same(S57ObjectCatalogue.InlandEnc, S57ObjectCatalogue.For(new S57DataSetIdentification { ProductSpecification = 10 }));
        Assert.Throws<ArgumentNullException>(() => S57ObjectCatalogue.For(null!));
    }

    [Fact]
    public void ToString_DescribesEntries()
    {
        Assert.Equal("LIGHTS (75): Light", S57ObjectCatalogue.Iho.GetObjectClass(75)?.ToString());
        Assert.Equal("COLOUR (75): Colour", S57ObjectCatalogue.Iho.GetAttribute(75)?.ToString());
        Assert.Equal("1: white", S57ObjectCatalogue.Iho.GetAttribute(75)?.GetValue(1)?.ToString());
        Assert.Equal("IHO S-57 Edition 3.1", S57ObjectCatalogue.Iho.ToString());
    }
}
