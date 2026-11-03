using EncDotNet.S57;

namespace EncDotNet.UnitTests;

public class S57LexicalLevelsTests
{
    [Fact]
    public void Lowest_OfNoValues_IsAscii()
    {
        Assert.Equal(S57LexicalLevel.Ascii, S57LexicalLevels.Lowest([]));
    }

    [Fact]
    public void Lowest_OfAsciiValues_IsAscii()
    {
        Assert.Equal(S57LexicalLevel.Ascii, S57LexicalLevels.Lowest(["North sea 1", "", null, "~\u007F"]));
    }

    [Fact]
    public void Lowest_OfLatin1Values_IsLatin1()
    {
        Assert.Equal(S57LexicalLevel.Latin1, S57LexicalLevels.Lowest(["North sea 1", "Bouée du Chêne 45°", "ÿ"]));
    }

    [Fact]
    public void Lowest_OfBmpValues_IsUcs2()
    {
        // The Latin-1 value after the Greek one doesn't lower the level.
        Assert.Equal(S57LexicalLevel.Ucs2, S57LexicalLevels.Lowest(["Noordzee", "Βόρεια Θάλασσα", "Chêne"]));
        Assert.Equal(S57LexicalLevel.Ucs2, S57LexicalLevels.Lowest(["Ā"]));
        Assert.Equal(S57LexicalLevel.Ucs2, S57LexicalLevels.Lowest(["�"]));
    }

    [Fact]
    public void Lowest_OfNonBmpValue_Throws()
    {
        // Built here rather than as theory data, which would replace unpaired surrogates.
        string[] values =
        [
            "Harbour \U0001F6A2", // A surrogate pair.
            "\uD800", // An unpaired high surrogate.
            "\uDC00" // An unpaired low surrogate.
        ];

        foreach (var value in values)
        {
            Assert.Throws<ArgumentException>(() => S57LexicalLevels.Lowest(["ASCII", value]));
        }
    }

    [Fact]
    public void Lowest_OfNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => S57LexicalLevels.Lowest(null!));
    }

    [Theory]
    [InlineData("abc", S57LexicalLevel.Ascii, true)]
    [InlineData("é", S57LexicalLevel.Ascii, false)]
    [InlineData("é", S57LexicalLevel.Latin1, true)]
    [InlineData("Ω", S57LexicalLevel.Latin1, false)]
    [InlineData("Ω", S57LexicalLevel.Ucs2, true)]
    [InlineData("\U0001F6A2", S57LexicalLevel.Ucs2, false)]
    [InlineData(null, S57LexicalLevel.Ascii, true)]
    public void CanEncode_ChecksTheCharacterSet(string? value, S57LexicalLevel level, bool expected)
    {
        Assert.Equal(expected, S57LexicalLevels.CanEncode(value, level));
    }

    [Fact]
    public void CanEncode_AtAnUndefinedLevel_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => S57LexicalLevels.CanEncode("a", (S57LexicalLevel)3));
    }

    [Fact]
    public void Lowest_AgreesWithTheWriter()
    {
        // A value one level up from the one picked can't be written at the level below it.
        var cell = S57WriterTestCells.SmallCell();
        var natf = cell.FeatureRecords.SelectMany(f => f.NationalAttributes).Select(a => a.Value).ToList();
        var level = S57LexicalLevels.Lowest(natf);

        Assert.Equal(S57LexicalLevel.Ucs2, level);
        Assert.Throws<ArgumentException>(() => S57DocumentWriter.Write(S57WriterTestCells.SmallCell(natfLexicalLevel: 1)));
    }

    [Fact]
    public void WithLowestLevels_WritesTheGoldenCell()
    {
        // The golden cell has Latin-1 ATTF values and Greek NATF values, at AALL 1 and NALL 2.
        var cell = S57LexicalLevels.WithLowestLevels(S57WriterTestCells.SmallCell(0, 0));

        Assert.Equal(1, cell.DataSetIdentification!.AttfLexicalLevel);
        Assert.Equal(2, cell.DataSetIdentification.NatfLexicalLevel);
        Assert.Equal(S57DocumentWriter.Write(S57WriterTestCells.SmallCell()), S57DocumentWriter.Write(cell));
    }

    [Theory]
    [InlineData("North sea 1", "Noordzee 1", S57LexicalLevel.Ascii, S57LexicalLevel.Ascii)]
    [InlineData("Mer du Nord °", "Noordzee 1", S57LexicalLevel.Latin1, S57LexicalLevel.Ascii)]
    [InlineData("North sea 1", "Nordsee ü", S57LexicalLevel.Ascii, S57LexicalLevel.Latin1)]
    [InlineData("Mer du Nord °", "Βόρεια Θάλασσα", S57LexicalLevel.Latin1, S57LexicalLevel.Ucs2)]
    public void WithLowestLevels_RoundTripsThroughTheWriter(
        string attf, string natf, S57LexicalLevel expectedAttf, S57LexicalLevel expectedNatf)
    {
        var cell = WithText(S57WriterTestCells.SmallCell(1, 2), attf, natf);

        var lowest = S57LexicalLevels.WithLowestLevels(cell);
        var read = S57DocumentReader.Read(S57DocumentWriter.Write(lowest));

        Assert.Equal(expectedAttf, (S57LexicalLevel)read.DataSetIdentification!.AttfLexicalLevel);
        Assert.Equal(expectedNatf, (S57LexicalLevel)read.DataSetIdentification.NatfLexicalLevel);
        Assert.Equal(lowest.FeatureRecords.SelectMany(f => f.Attributes), read.FeatureRecords.SelectMany(f => f.Attributes));
        Assert.Equal(lowest.FeatureRecords.SelectMany(f => f.NationalAttributes), read.FeatureRecords.SelectMany(f => f.NationalAttributes));
    }

    [Fact]
    public void WithLowestLevels_AttfNeedingUcs2_Throws()
    {
        var cell = WithText(S57WriterTestCells.SmallCell(), "Βόρεια Θάλασσα", "Noordzee");

        Assert.Throws<ArgumentException>(() => S57LexicalLevels.WithLowestLevels(cell));
    }

    [Fact]
    public void WithLowestLevels_WithoutDsid_Throws()
    {
        Assert.Throws<ArgumentException>(() => S57LexicalLevels.WithLowestLevels(new S57Document()));
    }

    // Replaces every ATTF and NATF value with one value each, keeping the attribute codes.
    private static S57Document WithText(S57Document cell, string attf, string natf) => cell with
    {
        FeatureRecords = cell.FeatureRecords.Select(f => f with
        {
            Attributes = f.Attributes.Select(a => a with { Value = attf }).ToList(),
            NationalAttributes = f.NationalAttributes.Select(a => a with { Value = natf }).ToList()
        }).ToList()
    };
}
