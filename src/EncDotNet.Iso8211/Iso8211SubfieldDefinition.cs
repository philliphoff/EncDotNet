namespace EncDotNet.Iso8211;

/// <summary>
/// Represents a single subfield definition within a field definition.
/// </summary>
/// <remarks>
/// A subfield definition specifies the name and format of an individual subfield
/// within an ISO 8211 field. The format determines how to read the subfield data
/// from binary field data.
/// </remarks>
public sealed class Iso8211SubfieldDefinition
{
    /// <summary>
    /// Gets the subfield name/label (e.g., "RCNM", "RCID", "DSNM").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the format specification for this subfield.
    /// </summary>
    public Iso8211SubfieldFormat Format { get; init; }

    /// <summary>
    /// Gets the index of this subfield within its parent field definition.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Gets whether this subfield is part of a repeating group.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, this subfield and all subsequent subfields in the parent field
    /// definition form a group that may repeat multiple times within the field data.
    /// The group is indicated by a <c>*</c> prefix in the DDR descriptor.
    /// </remarks>
    public bool IsRepeating { get; init; }
}
