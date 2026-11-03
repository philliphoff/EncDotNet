using EncDotNet.S57;
using EncDotNet.S57.Aml;

namespace EncDotNet.UnitTests;

public class S57ProductIdentityTests
{
    [Theory]
    [InlineData(1, S57ProductKind.Enc)]
    [InlineData(2, S57ProductKind.DataDictionary)]
    [InlineData(10, S57ProductKind.InlandEnc)]
    [InlineData(0, S57ProductKind.Unknown)]
    [InlineData(3, S57ProductKind.Unknown)]
    [InlineData(50, S57ProductKind.Unknown)]
    [InlineData(58, S57ProductKind.Unknown)]
    [InlineData(-1, S57ProductKind.Unknown)]
    public void NonAmlProductSpecification_MapsToKind(int prsp, S57ProductKind expected)
    {
        var product = Identification(prsp: prsp).Product;

        Assert.Equal(expected, product.Kind);
        Assert.Null(product.AmlProduct);
    }

    [Theory]
    [InlineData(51, AmlProduct.MFF)]
    [InlineData(52, AmlProduct.RAL)]
    [InlineData(53, AmlProduct.LBO)]
    [InlineData(54, AmlProduct.CLB)]
    [InlineData(55, AmlProduct.ESB)]
    [InlineData(56, AmlProduct.SBO)]
    [InlineData(57, AmlProduct.Integrated)]
    public void AmlProductSpecification_MapsToAmlProduct(int prsp, AmlProduct expected)
    {
        var product = Identification(prsp: prsp).Product;

        Assert.Equal(S57ProductKind.Aml, product.Kind);
        Assert.Equal(expected, product.AmlProduct);
    }

    [Theory]
    [InlineData("1.0", 1, 0)]
    [InlineData("2.1", 2, 1)]
    [InlineData("3.0", 3, 0)]
    [InlineData("2.0", 2, 0)]
    [InlineData(" 3.0 ", 3, 0)]
    [InlineData("3", 3, 0)]
    public void Edition_ParsesAsVersion(string pred, int major, int minor)
    {
        var product = Identification(pred: pred).Product;

        Assert.Equal(pred, product.Edition);
        Assert.Equal(new Version(major, minor), product.Version);
    }

    [Theory]
    [InlineData("")]
    [InlineData("draft")]
    [InlineData("v2.0")]
    public void UnparseableEdition_HasNoVersion(string pred)
    {
        var product = Identification(pred: pred).Product;

        Assert.Equal(pred, product.Edition);
        Assert.Null(product.Version);
    }

    [Theory]
    [InlineData(1, false)] // EN
    [InlineData(2, true)] // ER
    [InlineData(4, false)] // CLB base
    [InlineData(5, true)] // CLB update
    [InlineData(6, false)] // ESB base
    [InlineData(7, true)] // ESB update
    [InlineData(10, false)] // LBO base
    [InlineData(11, true)] // LBO update
    [InlineData(12, false)] // MFF base
    [InlineData(13, true)] // MFF update
    [InlineData(16, false)] // RAL base
    [InlineData(17, true)] // RAL update
    [InlineData(18, false)] // SBO base
    [InlineData(19, true)] // SBO update
    public void ApplicationProfile_DecidesIsUpdate(int prof, bool expected)
    {
        // The data set name and exchange purpose disagree with the profile, so only PROF can
        // give the expected answer.
        var identification = Identification(prof: prof) with
        {
            DataSetName = expected ? "GB5X0001.000" : "GB5X0001.003",
            ExchangePurpose = expected ? 1 : 2
        };

        Assert.Equal(expected, identification.Product.IsUpdate);
    }

    [Theory]
    [InlineData(20, "GB4MFF01.000", 1, false)] // AML 3.0 base
    [InlineData(20, "GB4MFF01.001", 2, true)] // AML 3.0 update
    [InlineData(20, "GB4MFF01.012", 1, true)] // the name wins over EXPP
    [InlineData(3, "ODD00001.000", 1, false)] // DD
    [InlineData(0, "XX5CELL.000", 2, false)]
    [InlineData(0, "XX5CELL.002", 1, true)]
    [InlineData(0, "XX5CELL", 1, false)] // no extension: EXPP
    [InlineData(0, "XX5CELL", 2, true)]
    [InlineData(0, "XX5CELL.ABC", 2, true)] // non-numeric extension: EXPP
    [InlineData(0, "", 0, false)]
    [InlineData(99, "", 2, true)] // unknown PROF: EXPP
    public void ApplicationProfileWithoutBaseOrUpdate_FallsBack(int prof, string dsnm, int expp, bool expected)
    {
        var identification = Identification(prof: prof) with { DataSetName = dsnm, ExchangePurpose = expp };

        Assert.Equal(expected, identification.Product.IsUpdate);
    }

    [Fact]
    public void Default_IsUnknownWithEmptyEdition()
    {
        var product = default(S57ProductIdentity);

        Assert.Equal(S57ProductKind.Unknown, product.Kind);
        Assert.Null(product.AmlProduct);
        Assert.Equal(string.Empty, product.Edition);
        Assert.Null(product.Version);
        Assert.False(product.IsUpdate);
    }

    [Fact]
    public void FromDataSetIdentification_NullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => S57ProductIdentity.FromDataSetIdentification(null!));
    }

    [Fact]
    public void Product_DoesNotAffectDataSetIdentificationEquality()
    {
        Assert.Equal(Identification(), Identification());
    }

    [Theory]
    [InlineData(51, "1.0", 12, "GB1MFF01.000", AmlProduct.MFF, false)]
    [InlineData(52, "2.1", 17, "GB1RAL01.001", AmlProduct.RAL, true)]
    [InlineData(57, "3.0", 20, "GB4AML01.000", AmlProduct.Integrated, false)]
    public void SyntheticAmlCell_ReadsProductIdentity(
        int prsp, string pred, int prof, string dsnm, AmlProduct expectedProduct, bool expectedUpdate)
    {
        var cell = S57WriterTestCells.SmallCell();
        cell = cell with
        {
            DataSetIdentification = cell.DataSetIdentification! with
            {
                DataSetName = dsnm,
                ProductSpecification = prsp,
                ProductSpecificationEdition = pred,
                ApplicationProfile = prof,
                ExchangePurpose = expectedUpdate ? 2 : 1,
                UpdateNumber = expectedUpdate ? "1" : "0"
            }
        };

        var read = S57DocumentReader.Read(S57DocumentWriter.Write(cell));

        var product = read.DataSetIdentification?.Product;
        Assert.NotNull(product);
        Assert.Equal(S57ProductKind.Aml, product.Value.Kind);
        Assert.Equal(expectedProduct, product.Value.AmlProduct);
        Assert.Equal(pred, product.Value.Edition);
        Assert.Equal(Version.Parse(pred), product.Value.Version);
        Assert.Equal(expectedUpdate, product.Value.IsUpdate);
    }

    [Fact]
    public void SmallCell_IsAnEncBase()
    {
        var read = S57DocumentReader.Read(S57DocumentWriter.Write(S57WriterTestCells.SmallCell()));

        var product = read.DataSetIdentification?.Product;
        Assert.Equal(S57ProductKind.Enc, product?.Kind);
        Assert.Equal(new Version(2, 0), product?.Version);
        Assert.False(product?.IsUpdate);
    }

    private static S57DataSetIdentification Identification(int prsp = 1, string pred = "2.0", int prof = 1) => new()
    {
        DataSetName = "XX5CELL.000",
        ExchangePurpose = 1,
        ProductSpecification = prsp,
        ProductSpecificationEdition = pred,
        ApplicationProfile = prof
    };
}
