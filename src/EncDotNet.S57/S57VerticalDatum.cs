namespace EncDotNet.S57;

/// <summary>
/// The vertical datums of the DSPM subfields VDAT and SDAT, which take their values from the
/// attribute <c>VERDAT</c> (S-57 Edition 3.1, Part 3 §7.3.2.1; Appendix A, chapter 2,
/// attribute 185).
/// </summary>
/// <remarks>
/// The values are the exact S-57 codes, so cast between this enum and
/// <see cref="S57DataSetParameters.VerticalDatum"/>, <see cref="S57DataSetParameters.SoundingDatum"/>
/// (or a <c>VERDAT</c> attribute value) in either direction. A code S-57 doesn't define casts to
/// an unnamed value rather than failing.
/// </remarks>
public enum S57VerticalDatum
{
    /// <summary>Mean low water springs (code 1).</summary>
    MeanLowWaterSprings = 1,

    /// <summary>Mean lower low water springs (code 2).</summary>
    MeanLowerLowWaterSprings = 2,

    /// <summary>Mean sea level (code 3).</summary>
    MeanSeaLevel = 3,

    /// <summary>Lowest low water (code 4).</summary>
    LowestLowWater = 4,

    /// <summary>Mean low water (code 5).</summary>
    MeanLowWater = 5,

    /// <summary>Lowest low water springs (code 6).</summary>
    LowestLowWaterSprings = 6,

    /// <summary>Approximate mean low water springs (code 7).</summary>
    ApproximateMeanLowWaterSprings = 7,

    /// <summary>Indian spring low water (code 8).</summary>
    IndianSpringLowWater = 8,

    /// <summary>Low water springs (code 9).</summary>
    LowWaterSprings = 9,

    /// <summary>Approximate lowest astronomical tide (code 10).</summary>
    ApproximateLowestAstronomicalTide = 10,

    /// <summary>Nearly lowest low water (code 11).</summary>
    NearlyLowestLowWater = 11,

    /// <summary>Mean lower low water (code 12).</summary>
    MeanLowerLowWater = 12,

    /// <summary>Low water (code 13).</summary>
    LowWater = 13,

    /// <summary>Approximate mean low water (code 14).</summary>
    ApproximateMeanLowWater = 14,

    /// <summary>Approximate mean lower low water (code 15).</summary>
    ApproximateMeanLowerLowWater = 15,

    /// <summary>Mean high water (code 16).</summary>
    MeanHighWater = 16,

    /// <summary>Mean high water springs (code 17).</summary>
    MeanHighWaterSprings = 17,

    /// <summary>High water (code 18).</summary>
    HighWater = 18,

    /// <summary>Approximate mean sea level (code 19).</summary>
    ApproximateMeanSeaLevel = 19,

    /// <summary>High water springs (code 20).</summary>
    HighWaterSprings = 20,

    /// <summary>Mean higher high water (code 21).</summary>
    MeanHigherHighWater = 21,

    /// <summary>Equinoctial spring low water (code 22).</summary>
    EquinoctialSpringLowWater = 22,

    /// <summary>Lowest astronomical tide (code 23).</summary>
    LowestAstronomicalTide = 23,

    /// <summary>Local datum (code 24).</summary>
    LocalDatum = 24,

    /// <summary>International Great Lakes Datum 1985 (code 25).</summary>
    InternationalGreatLakesDatum1985 = 25,

    /// <summary>Mean water level (code 26).</summary>
    MeanWaterLevel = 26,

    /// <summary>Lower low water large tide (code 27).</summary>
    LowerLowWaterLargeTide = 27,

    /// <summary>Higher high water large tide (code 28).</summary>
    HigherHighWaterLargeTide = 28,

    /// <summary>Nearly highest high water (code 29).</summary>
    NearlyHighestHighWater = 29,

    /// <summary>Highest astronomical tide (HAT) (code 30).</summary>
    HighestAstronomicalTide = 30
}
