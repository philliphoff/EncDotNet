namespace EncDotNet.S57;

/// <summary>
/// The coordinate unit codes of the DSPM subfield COUN (S-57 Edition 3.1, Part 3 §7.3.2.1 and
/// clause 3.2.1).
/// </summary>
/// <remarks>
/// The values are the exact S-57 codes, so cast between this enum and
/// <see cref="S57DataSetParameters.CoordinateUnits"/> in either direction. A code S-57 doesn't
/// define casts to an unnamed value rather than failing.
/// </remarks>
public enum S57CoordinateUnits
{
    /// <summary>Latitude and longitude (<c>LL</c>, code 1).</summary>
    LatitudeLongitude = 1,

    /// <summary>Easting and northing (<c>EN</c>, code 2).</summary>
    EastingNorthing = 2,

    /// <summary>Units on the chart or map (<c>UC</c>, code 3).</summary>
    UnitsOnChart = 3
}
