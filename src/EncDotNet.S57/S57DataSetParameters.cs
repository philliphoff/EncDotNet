namespace EncDotNet.S57;

/// <summary>
/// Represents the Data Set Parameter field (DSPM) from S-57.
/// </summary>
public sealed class S57DataSetParameters
{
    /// <summary>Gets the record name.</summary>
    public S57RecordName RecordName { get; init; }

    /// <summary>Gets the horizontal geodetic datum (HDAT).</summary>
    public int HorizontalDatum { get; init; }

    /// <summary>Gets the vertical datum (VDAT).</summary>
    public int VerticalDatum { get; init; }

    /// <summary>Gets the sounding datum (SDAT).</summary>
    public int SoundingDatum { get; init; }

    /// <summary>Gets the compilation scale (CSCL).</summary>
    public int CompilationScale { get; init; }

    /// <summary>Gets the units of depth measurement (DUNI).</summary>
    public int DepthUnits { get; init; }

    /// <summary>Gets the units of height measurement (HUNI).</summary>
    public int HeightUnits { get; init; }

    /// <summary>Gets the units of positional accuracy (PUNI).</summary>
    public int PositionalUnits { get; init; }

    /// <summary>Gets the coordinate units (COUN).</summary>
    public int CoordinateUnits { get; init; }

    /// <summary>Gets the coordinate multiplication factor (COMF).</summary>
    public int CoordinateMultiplicationFactor { get; init; }

    /// <summary>Gets the sounding multiplication factor (SOMF).</summary>
    public int SoundingMultiplicationFactor { get; init; }

    /// <summary>Gets the comment (COMT).</summary>
    public string Comment { get; init; } = string.Empty;
}
