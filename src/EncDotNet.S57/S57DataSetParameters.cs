namespace EncDotNet.S57;

/// <summary>
/// Represents the Data Set Parameter field (DSPM) from S-57.
/// </summary>
public sealed record S57DataSetParameters
{
    /// <summary>Gets the record name.</summary>
    public S57RecordName RecordName { get; init; }

    /// <summary>Gets the horizontal geodetic datum (HDAT).</summary>
    /// <remarks>
    /// The values are those of the attribute <c>HORDAT</c>, which <see cref="S57HorizontalDatum"/>
    /// names; cast between the two. 2 is WGS 84.
    /// </remarks>
    public int HorizontalDatum { get; init; }

    /// <summary>Gets the vertical datum (VDAT).</summary>
    /// <remarks>
    /// The values are those of the attribute <c>VERDAT</c>, which <see cref="S57VerticalDatum"/>
    /// names; cast between the two.
    /// </remarks>
    public int VerticalDatum { get; init; }

    /// <summary>Gets the sounding datum (SDAT).</summary>
    /// <remarks>
    /// The values are those of the attribute <c>VERDAT</c>, which <see cref="S57VerticalDatum"/>
    /// names; cast between the two.
    /// </remarks>
    public int SoundingDatum { get; init; }

    /// <summary>Gets the compilation scale (CSCL).</summary>
    public int CompilationScale { get; init; }

    /// <summary>Gets the units of depth measurement (DUNI).</summary>
    /// <remarks>
    /// The values are those of the attribute <c>DUNITS</c>, which <see cref="S57DepthUnits"/> names;
    /// cast between the two.
    /// </remarks>
    public int DepthUnits { get; init; }

    /// <summary>Gets the units of height measurement (HUNI).</summary>
    /// <remarks>
    /// The values are those of the attribute <c>HUNITS</c>, which <see cref="S57HeightUnits"/> names;
    /// cast between the two.
    /// </remarks>
    public int HeightUnits { get; init; }

    /// <summary>Gets the units of positional accuracy (PUNI).</summary>
    /// <remarks>
    /// The values are those of the attribute <c>PUNITS</c>, which
    /// <see cref="S57PositionalAccuracyUnits"/> names; cast between the two.
    /// </remarks>
    public int PositionalUnits { get; init; }

    /// <summary>Gets the coordinate units (COUN).</summary>
    /// <remarks>
    /// <see cref="S57CoordinateUnits"/> names the codes of S-57 Edition 3.1, Part 3 §7.3.2.1; cast
    /// between the two.
    /// </remarks>
    public int CoordinateUnits { get; init; }

    /// <summary>Gets the coordinate multiplication factor (COMF).</summary>
    public int CoordinateMultiplicationFactor { get; init; }

    /// <summary>Gets the sounding multiplication factor (SOMF).</summary>
    public int SoundingMultiplicationFactor { get; init; }

    /// <summary>Gets the comment (COMT).</summary>
    /// <remarks>
    /// S-57 makes this basic text (ASCII), but some producers, such as NATO AML cells, put
    /// ISO 8859-1 characters in it, so it's read and written as ISO 8859-1, a superset of ASCII.
    /// </remarks>
    public string Comment { get; init; } = string.Empty;
}
