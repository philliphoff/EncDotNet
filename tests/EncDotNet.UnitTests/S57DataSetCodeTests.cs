using EncDotNet.S57;

namespace EncDotNet.UnitTests;

// The codes are those of S-57 Edition 3.1, Part 3 §7.3.1.1, §7.3.1.2 and §7.3.2.1, and the
// attribute domains of Appendix A, chapter 2.
public class S57DataSetCodeTests
{
    [Fact]
    public void ExchangePurpose_MatchesTheSpecification()
    {
        Assert.Equal(1, (int)S57ExchangePurpose.New);
        Assert.Equal(2, (int)S57ExchangePurpose.Revision);
        Assert.Equal(2, Enum.GetValues<S57ExchangePurpose>().Length);
    }

    [Fact]
    public void ProductSpecification_MatchesTheSpecification()
    {
        Assert.Equal(1, (int)S57ProductSpecification.Enc);
        Assert.Equal(2, (int)S57ProductSpecification.DataDictionary);
        Assert.Equal(10, (int)S57ProductSpecification.InlandEnc);
        Assert.Equal(3, Enum.GetValues<S57ProductSpecification>().Length);
    }

    [Theory]
    [InlineData(S57ProductSpecification.Enc, S57ProductKind.Enc)]
    [InlineData(S57ProductSpecification.DataDictionary, S57ProductKind.DataDictionary)]
    [InlineData(S57ProductSpecification.InlandEnc, S57ProductKind.InlandEnc)]
    [InlineData((S57ProductSpecification)(int)EncDotNet.S57.Aml.AmlProduct.RAL, S57ProductKind.Aml)]
    public void ProductSpecification_IsInterpretedAsItsProductKind(S57ProductSpecification code, S57ProductKind kind)
    {
        var dsid = new S57DataSetIdentification { ProductSpecification = (int)code };

        Assert.Equal(kind, dsid.Product.Kind);
    }

    [Fact]
    public void ApplicationProfile_MatchesTheSpecification()
    {
        Assert.Equal(1, (int)S57ApplicationProfile.EncNew);
        Assert.Equal(2, (int)S57ApplicationProfile.EncRevision);
        Assert.Equal(3, (int)S57ApplicationProfile.DataDictionary);
        Assert.Equal(3, Enum.GetValues<S57ApplicationProfile>().Length);
    }

    [Fact]
    public void DataStructure_MatchesTheSpecification()
    {
        Assert.Equal(1, (int)S57DataStructure.CartographicSpaghetti);
        Assert.Equal(2, (int)S57DataStructure.ChainNode);
        Assert.Equal(3, (int)S57DataStructure.PlanarGraph);
        Assert.Equal(4, (int)S57DataStructure.FullTopology);
        Assert.Equal(255, (int)S57DataStructure.TopologyNotRelevant);
        Assert.Equal(5, Enum.GetValues<S57DataStructure>().Length);
    }

    [Fact]
    public void CoordinateUnits_MatchesTheSpecification()
    {
        Assert.Equal(1, (int)S57CoordinateUnits.LatitudeLongitude);
        Assert.Equal(2, (int)S57CoordinateUnits.EastingNorthing);
        Assert.Equal(3, (int)S57CoordinateUnits.UnitsOnChart);
        Assert.Equal(3, Enum.GetValues<S57CoordinateUnits>().Length);
    }

    [Fact]
    public void DepthUnits_MatchesDunits()
    {
        Assert.Equal(1, (int)S57DepthUnits.Metres);
        Assert.Equal(2, (int)S57DepthUnits.FathomsAndFeet);
        Assert.Equal(3, (int)S57DepthUnits.Feet);
        Assert.Equal(4, (int)S57DepthUnits.FathomsAndFractions);
        Assert.Equal(4, Enum.GetValues<S57DepthUnits>().Length);
    }

    [Fact]
    public void HeightUnits_MatchesHunits()
    {
        Assert.Equal(1, (int)S57HeightUnits.Metres);
        Assert.Equal(2, (int)S57HeightUnits.Feet);
        Assert.Equal(2, Enum.GetValues<S57HeightUnits>().Length);
    }

    [Fact]
    public void PositionalAccuracyUnits_MatchesPunits()
    {
        Assert.Equal(1, (int)S57PositionalAccuracyUnits.Metres);
        Assert.Equal(2, (int)S57PositionalAccuracyUnits.DegreesOfArc);
        Assert.Equal(3, (int)S57PositionalAccuracyUnits.Millimetres);
        Assert.Equal(4, (int)S57PositionalAccuracyUnits.Feet);
        Assert.Equal(5, (int)S57PositionalAccuracyUnits.Cables);
        Assert.Equal(5, Enum.GetValues<S57PositionalAccuracyUnits>().Length);
    }

    [Fact]
    public void HorizontalDatum_MatchesHordat()
    {
        Assert.Equal(1, (int)S57HorizontalDatum.WGS72);
        Assert.Equal(2, (int)S57HorizontalDatum.WGS84);
        Assert.Equal(3, (int)S57HorizontalDatum.European1950);
        Assert.Equal(74, (int)S57HorizontalDatum.NorthAmerican1927);
        Assert.Equal(75, (int)S57HorizontalDatum.NorthAmerican1983);
        Assert.Equal(127, (int)S57HorizontalDatum.GeocentricDatumOfAustralia);
        Assert.Equal(131, (int)S57HorizontalDatum.LocalDatum);

        // HORDAT's domain is 1 to 131, with no gaps.
        Assert.Equal(Enumerable.Range(1, 131), Enum.GetValues<S57HorizontalDatum>().Select(v => (int)v));
    }

    [Fact]
    public void VerticalDatum_MatchesVerdat()
    {
        Assert.Equal(1, (int)S57VerticalDatum.MeanLowWaterSprings);
        Assert.Equal(3, (int)S57VerticalDatum.MeanSeaLevel);
        Assert.Equal(12, (int)S57VerticalDatum.MeanLowerLowWater);
        Assert.Equal(23, (int)S57VerticalDatum.LowestAstronomicalTide);
        Assert.Equal(24, (int)S57VerticalDatum.LocalDatum);
        Assert.Equal(30, (int)S57VerticalDatum.HighestAstronomicalTide);

        // VERDAT's domain is 1 to 30, with no gaps.
        Assert.Equal(Enumerable.Range(1, 30), Enum.GetValues<S57VerticalDatum>().Select(v => (int)v));
    }

    [Fact]
    public void LexicalLevel_MatchesTheSpecification()
    {
        Assert.Equal(0, (int)S57LexicalLevel.Ascii);
        Assert.Equal(1, (int)S57LexicalLevel.Latin1);
        Assert.Equal(2, (int)S57LexicalLevel.Ucs2);
    }

    [Fact]
    public void UnknownCodes_RoundTripThroughTheEnums()
    {
        var parameters = new S57DataSetParameters { HorizontalDatum = 200, CoordinateUnits = 9 };

        var datum = (S57HorizontalDatum)parameters.HorizontalDatum;
        var units = (S57CoordinateUnits)parameters.CoordinateUnits;

        Assert.False(Enum.IsDefined(datum));
        Assert.Equal(200, (int)datum);
        Assert.Equal(9, (int)units);
    }

    [Fact]
    public void Codes_SetFromTheEnums_RoundTripThroughTheWriter()
    {
        var cell = S57WriterTestCells.SmallCell();
        cell = cell with
        {
            DataSetIdentification = cell.DataSetIdentification! with
            {
                ExchangePurpose = (int)S57ExchangePurpose.New,
                ProductSpecification = (int)S57ProductSpecification.Enc,
                ApplicationProfile = (int)S57ApplicationProfile.EncNew,
                DataStructure = (int)S57DataStructure.ChainNode
            },
            DataSetParameters = cell.DataSetParameters! with
            {
                HorizontalDatum = (int)S57HorizontalDatum.WGS84,
                VerticalDatum = (int)S57VerticalDatum.MeanSeaLevel,
                SoundingDatum = (int)S57VerticalDatum.LowestAstronomicalTide,
                DepthUnits = (int)S57DepthUnits.Metres,
                HeightUnits = (int)S57HeightUnits.Metres,
                PositionalUnits = (int)S57PositionalAccuracyUnits.Metres,
                CoordinateUnits = (int)S57CoordinateUnits.LatitudeLongitude
            }
        };

        var read = S57DocumentReader.Read(S57DocumentWriter.Write(cell));

        // The small cell already sets these codes, so it writes the same bytes as the golden.
        Assert.Equal(S57DocumentWriter.Write(S57WriterTestCells.SmallCell()), S57DocumentWriter.Write(cell));
        Assert.Equal(S57DataStructure.ChainNode, (S57DataStructure)read.DataSetIdentification!.DataStructure);
        Assert.Equal(S57HorizontalDatum.WGS84, (S57HorizontalDatum)read.DataSetParameters!.HorizontalDatum);
        Assert.Equal(S57VerticalDatum.LowestAstronomicalTide, (S57VerticalDatum)read.DataSetParameters.SoundingDatum);
    }
}
