namespace EncDotNet.Iso8211;

/// <summary>
/// Specifies the data type code for an ISO 8211 field definition.
/// </summary>
/// <remarks>
/// This code indicates the general type of data contained in the field.
/// It is encoded as the second character of the field controls in the DDR.
/// </remarks>
public enum Iso8211DataTypeCode : byte
{
    /// <summary>
    /// Character string data.
    /// </summary>
    CharacterString = 0,

    /// <summary>
    /// Implicit point representation (integer interpreted as real with implicit decimal).
    /// </summary>
    ImplicitPoint = 1,

    /// <summary>
    /// Explicit point representation (real number with explicit decimal point).
    /// </summary>
    ExplicitPoint = 2,

    /// <summary>
    /// Binary data.
    /// </summary>
    Binary = 5,

    /// <summary>
    /// Mixed data types — subfields may have different data types.
    /// </summary>
    MixedDataTypes = 6
}
