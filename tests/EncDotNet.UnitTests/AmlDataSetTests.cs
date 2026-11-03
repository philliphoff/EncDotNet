using EncDotNet.S57;
using EncDotNet.S57.Aml;
using EncDotNet.S57.Charts;

namespace EncDotNet.UnitTests;

public class AmlDataSetTests
{
    private const string Comment = "title=Wrecks 50°N; IDO Status = Final; Protective Marking = UNCLASSIFIED; Owner Authority = Test Authority";

    [Fact]
    public void FromChart_ExposesProductMetadataAndCatalogue()
    {
        var chart = S57Chart.FromDocument(AmlCell());

        var dataSet = new AmlDataSet(chart);

        Assert.Same(chart, dataSet.Chart);
        Assert.Same(chart.Identification, dataSet.Identification);
        Assert.Equal(S57ProductKind.Aml, dataSet.Product.Kind);
        Assert.Equal(AmlProduct.RAL, dataSet.AmlProduct);
        Assert.Same(S57ObjectCatalogue.Aml21, dataSet.Catalogue);
        Assert.NotNull(dataSet.Metadata);
        Assert.Equal("Wrecks 50°N", dataSet.Metadata.Title);
        Assert.Equal("UNCLASSIFIED", dataSet.Metadata.ProtectiveMarking);
    }

    [Fact]
    public void FromDocument_BuildsTheChart()
    {
        var dataSet = new AmlDataSet(AmlCell());

        Assert.Equal(5, dataSet.Chart.AllFeatures.Count);
        Assert.Equal(AmlProduct.RAL, dataSet.AmlProduct);
    }

    [Fact]
    public void FromStream_ReadsAWrittenCell()
    {
        using var stream = new MemoryStream(S57DocumentWriter.Write(AmlCell(prsp: 57, pred: "3.0")));

        var dataSet = AmlDataSet.FromStream(stream);

        Assert.Equal(AmlProduct.Integrated, dataSet.AmlProduct);
        Assert.Same(S57ObjectCatalogue.Aml30, dataSet.Catalogue);
        Assert.Equal("Final", dataSet.Metadata?.IdoStatus);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Produced by a test")]
    public void UnstructuredComment_HasNoMetadata(string comment)
    {
        var dataSet = new AmlDataSet(AmlCell(comment: comment));

        Assert.Null(dataSet.Metadata);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(58)]
    public void NonAmlDataSet_IsRejected(int prsp)
    {
        var document = AmlCell(prsp: prsp);
        var chart = S57Chart.FromDocument(document);

        Assert.Throws<ArgumentException>("document", () => new AmlDataSet(document));
        Assert.Throws<ArgumentException>("chart", () => new AmlDataSet(chart));
        Assert.False(AmlDataSet.TryCreate(document, out var fromDocument));
        Assert.Null(fromDocument);
        Assert.False(AmlDataSet.TryCreate(chart, out var fromChart));
        Assert.Null(fromChart);
    }

    [Fact]
    public void DataSetWithoutIdentification_IsRejected()
    {
        var document = AmlCell() with { DataSetIdentification = null };

        Assert.Throws<ArgumentException>("document", () => new AmlDataSet(document));
        Assert.False(AmlDataSet.TryCreate(S57Chart.FromDocument(document), out _));
    }

    [Fact]
    public void TryCreate_AcceptsAmlDataSets()
    {
        var document = AmlCell();

        Assert.True(AmlDataSet.TryCreate(document, out var fromDocument));
        Assert.Equal(AmlProduct.RAL, fromDocument.AmlProduct);
        Assert.True(AmlDataSet.TryCreate(S57Chart.FromDocument(document), out var fromChart));
        Assert.Equal(AmlProduct.RAL, fromChart.AmlProduct);
    }

    [Fact]
    public void NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new AmlDataSet((S57Chart)null!));
        Assert.Throws<ArgumentNullException>(() => new AmlDataSet((S57Document)null!));
        Assert.Throws<ArgumentNullException>(() => AmlDataSet.TryCreate((S57Chart)null!, out _));
        Assert.Throws<ArgumentNullException>(() => AmlDataSet.TryCreate((S57Document)null!, out _));
    }

    [Theory]
    [InlineData(51, "1.0", "AML 1.0")]
    [InlineData(54, "2.0", "AML 2.1")]
    [InlineData(52, "2.1", "AML 2.1")]
    [InlineData(57, "3.0", "AML 3.0")]
    [InlineData(57, "", "AML 3.0")]
    [InlineData(56, "", "AML 2.1")]
    public void Catalogue_FollowsTheAmlEdition(int prsp, string pred, string expected)
    {
        var dataSet = new AmlDataSet(AmlCell(prsp: prsp, pred: pred));

        Assert.Equal(expected, dataSet.Catalogue.Name);
    }

    [Fact]
    public void GetObjectClass_ResolvesAmlAndIhoCodes()
    {
        var dataSet = new AmlDataSet(AmlCell());

        Assert.Equal("atsctl", dataSet.GetObjectClass(Feature(dataSet, 1))?.Acronym);
        Assert.Equal("C_ASSO", dataSet.GetObjectClass(Feature(dataSet, 2))?.Acronym);
        Assert.Equal("M_COVR", dataSet.GetObjectClass(Feature(dataSet, 3))?.Acronym);
        Assert.Null(dataSet.GetObjectClass(Feature(dataSet, 4)));
    }

    [Fact]
    public void GetAttribute_ResolvesAmlAndIhoCodes()
    {
        var dataSet = new AmlDataSet(AmlCell());

        Assert.Equal("databa", dataSet.GetAttribute(new S57AttributeValue(20484, "20200101"))?.Acronym);
        Assert.Equal("OBJNAM", dataSet.GetAttribute(new S57AttributeValue(116, "Route"))?.Acronym);
        Assert.Null(dataSet.GetAttribute(new S57AttributeValue(39999, "")));
    }

    [Fact]
    public void GetAttributes_PairsValuesWithDefinitions()
    {
        var dataSet = new AmlDataSet(AmlCell());
        var feature = Feature(dataSet, 1);

        var attributes = dataSet.GetAttributes(feature);

        Assert.Collection(
            attributes,
            a =>
            {
                Assert.Equal(20484, a.Code);
                Assert.Equal("databa", a.Acronym);
                Assert.Equal("20200101", a.Value.Value);
            },
            a =>
            {
                Assert.Equal("arecat", a.Acronym);
                Assert.Equal("Solid Red (SR)", a.Definition?.GetValue(int.Parse(a.Value.Value, System.Globalization.CultureInfo.InvariantCulture))?.Name);
            },
            a =>
            {
                Assert.Equal(39999, a.Code);
                Assert.Null(a.Definition);
                Assert.Null(a.Acronym);
                Assert.Equal("39999=x", a.ToString());
            });

        var national = Assert.Single(dataSet.GetNationalAttributes(feature));
        Assert.Equal("NOBJNM", national.Acronym);
        Assert.Equal("NOBJNM=Ωμέγα", national.ToString());
    }

    [Fact]
    public void ToString_NamesTheCellProductAndCatalogue()
    {
        var dataSet = new AmlDataSet(AmlCell());

        Assert.Equal("XX5WRITE.000 (RAL, AML 2.1)", dataSet.ToString());
    }

    private static S57TypedFeature Feature(AmlDataSet dataSet, int id) =>
        dataSet.Chart.AllFeatures.Values.Single(f => f.RecordName.RecordId == id);

    /// <summary>
    /// The writer test cell, relabelled as an AML cell with AML and IHO codes. It's synthetic,
    /// not AML data.
    /// </summary>
    private static S57Document AmlCell(int prsp = 52, string pred = "2.1", string comment = Comment)
    {
        var cell = S57WriterTestCells.SmallCell();
        var features = cell.FeatureRecords;

        return cell with
        {
            DataSetIdentification = cell.DataSetIdentification! with
            {
                ProductSpecification = prsp,
                ProductSpecificationEdition = pred,
                ApplicationProfile = 16,
                Comment = comment
            },
            FeatureRecords =
            [
                features[0] with
                {
                    ObjectCode = (S57ObjectCode)20484, // atsctl
                    Attributes =
                    [
                        new S57AttributeValue(20484, "20200101"), // databa
                        new S57AttributeValue(20687, "501"),      // arecat
                        new S57AttributeValue(39999, "x")         // not in any catalogue
                    ],
                    NationalAttributes = [new S57AttributeValue(301, "Ωμέγα")] // NOBJNM
                },
                features[1] with { ObjectCode = S57ObjectCode.C_ASSO, Primitive = S57GeometricPrimitive.None },
                features[2] with { ObjectCode = S57ObjectCode.M_COVR },
                features[3] with { ObjectCode = (S57ObjectCode)39999 },
                features[4]
            ]
        };
    }
}
