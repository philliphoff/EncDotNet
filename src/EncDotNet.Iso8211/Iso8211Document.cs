using System.Collections.Immutable;

namespace EncDotNet.Iso8211;

/// <summary>
/// Represents a complete ISO 8211 document containing multiple records.
/// </summary>
public sealed record Iso8211Document
{
    /// <summary>
    /// Gets the records contained in this document.
    /// </summary>
    public IReadOnlyList<Iso8211Record> Records { get; init; } = [];

    /// <summary>
    /// Gets the Data Descriptive Record (DDR) if present.
    /// </summary>
    public Iso8211Record? DataDescriptiveRecord => Records.Count > 0 && Records[0].IsDataDescriptiveRecord ? Records[0] : null;

    /// <summary>
    /// Gets all data records (non-DDR records).
    /// </summary>
    public IEnumerable<Iso8211Record> DataRecords => Records.Where(r => !r.IsDataDescriptiveRecord);
}
