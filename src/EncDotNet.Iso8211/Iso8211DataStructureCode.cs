namespace EncDotNet.Iso8211;

/// <summary>
/// Specifies the data structure code for an ISO 8211 field definition.
/// </summary>
/// <remarks>
/// This code indicates how the data within the field is organized.
/// It is encoded as the first character of the field controls in the DDR.
/// </remarks>
public enum Iso8211DataStructureCode : byte
{
    /// <summary>
    /// Elementary field — contains a single data value or a set of subfields
    /// that appear exactly once.
    /// </summary>
    Elementary = 0,

    /// <summary>
    /// Vector field — contains a repeating set of subfields (a one-dimensional array).
    /// </summary>
    Vector = 1,

    /// <summary>
    /// Array field — contains a multi-dimensional array of subfields.
    /// </summary>
    Array = 2,

    /// <summary>
    /// Concatenated array field — contains subfields that may repeat as a group.
    /// </summary>
    ConcatenatedArray = 3
}
