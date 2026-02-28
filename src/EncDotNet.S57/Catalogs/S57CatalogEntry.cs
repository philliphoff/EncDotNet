namespace EncDotNet.S57.Catalogs;

/// <summary>
/// Represents a single entry in an S-57 CATALOG.031 file.
/// </summary>
/// <remarks>
/// Each entry corresponds to a CATD (Catalogue Directory) record in the catalog,
/// describing a chart file and its geographic coverage.
/// </remarks>
public sealed class S57CatalogEntry
{
    /// <summary>Gets the record name (RCNM), e.g. "CD" for Catalogue Directory.</summary>
    public string RecordName { get; init; } = "";

    /// <summary>Gets the record identification number (RCID).</summary>
    public uint RecordId { get; init; }

    /// <summary>Gets the file name.</summary>
    public string FileName { get; init; } = "";

    /// <summary>Gets the long file name.</summary>
    public string LongFileName { get; init; } = "";

    /// <summary>Gets the volume.</summary>
    public string Volume { get; init; } = "";

    /// <summary>Gets the implementation identifier (e.g. "ASC" for ASCII, "BIN" for binary).</summary>
    public string Implementation { get; init; } = "";

    /// <summary>Gets the southernmost latitude of the file's coverage area.</summary>
    public double? SouthernmostLatitude { get; init; }

    /// <summary>Gets the westernmost longitude of the file's coverage area.</summary>
    public double? WesternmostLongitude { get; init; }

    /// <summary>Gets the northernmost latitude of the file's coverage area.</summary>
    public double? NorthernmostLatitude { get; init; }

    /// <summary>Gets the easternmost longitude of the file's coverage area.</summary>
    public double? EasternmostLongitude { get; init; }

    /// <summary>Gets the CRC checksum of the referenced file.</summary>
    public string CrcChecksum { get; init; } = "";

    /// <summary>Gets the comment.</summary>
    public string Comment { get; init; } = "";
}
