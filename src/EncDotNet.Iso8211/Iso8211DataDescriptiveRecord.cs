using System.Collections.Immutable;

namespace EncDotNet.Iso8211;

/// <summary>
/// Represents a parsed ISO 8211 Data Descriptive Record (DDR).
/// </summary>
/// <remarks>
/// <para>
/// The DDR is the first record in an ISO 8211 file and describes the structure of all
/// subsequent data records. It contains field definitions that specify the names, types,
/// and formats of subfields within each field.
/// </para>
/// <para>
/// Use <see cref="Iso8211DdrParser.Parse(Iso8211Record)"/> to create an instance from
/// a raw <see cref="Iso8211Record"/>.
/// </para>
/// </remarks>
public sealed class Iso8211DataDescriptiveRecord
{
    /// <summary>
    /// Gets the field definitions contained in this DDR.
    /// </summary>
    /// <remarks>
    /// Each field definition describes the structure of a corresponding field in data records.
    /// The first entry (tag "0000") is the record directory field definition and is typically
    /// excluded from data record field lookups.
    /// </remarks>
    public ImmutableArray<Iso8211FieldDefinition> FieldDefinitions { get; init; }

    /// <summary>
    /// Gets a field definition by its tag.
    /// </summary>
    /// <param name="tag">The field tag to search for.</param>
    /// <returns>The field definition with the specified tag, or <c>null</c> if not found.</returns>
    public Iso8211FieldDefinition? GetFieldDefinition(string tag) =>
        FieldDefinitions.FirstOrDefault(f => f.Tag == tag);
}

/// <summary>
/// Represents a single field definition from the DDR.
/// </summary>
/// <remarks>
/// A field definition describes the structure and format of a field that appears in
/// data records. It includes the field tag, name, data structure type, data type,
/// and the definitions of all subfields within the field.
/// </remarks>
public sealed class Iso8211FieldDefinition
{
    /// <summary>
    /// Gets the field tag (e.g., "0001", "DSID", "FRID").
    /// </summary>
    public string Tag { get; init; } = string.Empty;

    /// <summary>
    /// Gets the data structure code for this field.
    /// </summary>
    public Iso8211DataStructureCode DataStructureCode { get; init; }

    /// <summary>
    /// Gets the data type code for this field.
    /// </summary>
    public Iso8211DataTypeCode DataTypeCode { get; init; }

    /// <summary>
    /// Gets the field name as specified in the DDR.
    /// </summary>
    public string FieldName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the array descriptor string, if present.
    /// </summary>
    /// <remarks>
    /// The array descriptor is used when the data structure is <see cref="Iso8211DataStructureCode.Array"/>
    /// or <see cref="Iso8211DataStructureCode.ConcatenatedArray"/>. It contains dimension information
    /// separated by <c>!</c> delimiters.
    /// </remarks>
    public string? ArrayDescriptor { get; init; }

    /// <summary>
    /// Gets the raw format controls string (e.g., "(A,I(10),b11)").
    /// </summary>
    public string FormatControls { get; init; } = string.Empty;

    /// <summary>
    /// Gets the subfield definitions for this field.
    /// </summary>
    public ImmutableArray<Iso8211SubfieldDefinition> SubfieldDefinitions { get; init; }

    /// <summary>
    /// Gets the index within <see cref="SubfieldDefinitions"/> at which the repeating
    /// subfield group begins, or <c>-1</c> if there is no repeating group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In ISO 8211, a <c>*</c> prefix on a subfield name in the DDR descriptor indicates
    /// that this subfield and all subsequent subfields form a repeating group. When a field
    /// has <see cref="DataStructureCode"/> of <see cref="Iso8211DataStructureCode.Vector"/>,
    /// the entire group of subfields from this index onward repeats until the end of the
    /// field data.
    /// </para>
    /// <para>
    /// For example, the S-57 SG2D field has descriptors <c>*YCOO!XCOO</c>, meaning both
    /// YCOO and XCOO repeat as a pair. In this case, <see cref="RepeatingSubfieldStartIndex"/>
    /// would be <c>0</c>.
    /// </para>
    /// <para>
    /// The S-57 FSPT field has descriptors <c>*NAME!ORNT!USAG!MASK</c>, meaning all four
    /// subfields repeat as a group, so <see cref="RepeatingSubfieldStartIndex"/> would be <c>0</c>.
    /// </para>
    /// </remarks>
    public int RepeatingSubfieldStartIndex { get; init; } = -1;

    /// <summary>
    /// Gets whether this field definition contains a repeating subfield group.
    /// </summary>
    public bool HasRepeatingGroup => RepeatingSubfieldStartIndex >= 0;

    /// <summary>
    /// Gets a subfield definition by its name/label.
    /// </summary>
    /// <param name="name">The subfield name to search for.</param>
    /// <returns>The subfield definition with the specified name, or <c>null</c> if not found.</returns>
    public Iso8211SubfieldDefinition? GetSubfieldDefinition(string name) =>
        SubfieldDefinitions.FirstOrDefault(s => s.Name == name);
}

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

/// <summary>
/// Describes the format of an ISO 8211 subfield.
/// </summary>
/// <remarks>
/// <para>
/// The format type and width determine how subfield data is encoded in data records.
/// Common format types used in S-57 ENC files include:
/// </para>
/// <list type="bullet">
/// <item><description><c>A</c> or <c>A(n)</c> — ASCII character data (variable or fixed length)</description></item>
/// <item><description><c>I(n)</c> — Integer as ASCII digits</description></item>
/// <item><description><c>R(n)</c> — Real number as ASCII</description></item>
/// <item><description><c>b11</c> — Unsigned 8-bit integer (1 byte)</description></item>
/// <item><description><c>b12</c> — Unsigned 16-bit integer (2 bytes)</description></item>
/// <item><description><c>b14</c> — Unsigned 32-bit integer (4 bytes)</description></item>
/// <item><description><c>b21</c> — Signed 8-bit integer (1 byte)</description></item>
/// <item><description><c>b22</c> — Signed 16-bit integer (2 bytes)</description></item>
/// <item><description><c>b24</c> — Signed 32-bit integer (4 bytes)</description></item>
/// </list>
/// </remarks>
public readonly struct Iso8211SubfieldFormat : IEquatable<Iso8211SubfieldFormat>
{
    /// <summary>
    /// Gets the format type of this subfield.
    /// </summary>
    public Iso8211SubfieldFormatType FormatType { get; init; }

    /// <summary>
    /// Gets the width (size) of this subfield in characters or bytes, or 0 for variable-length fields.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For character-based formats (<see cref="Iso8211SubfieldFormatType.CharacterData"/>,
    /// <see cref="Iso8211SubfieldFormatType.Integer"/>, <see cref="Iso8211SubfieldFormatType.Real"/>),
    /// this is the number of characters. A value of 0 indicates variable-length data terminated
    /// by a unit terminator (0x1F) or field terminator (0x1E).
    /// </para>
    /// <para>
    /// For binary formats (<see cref="Iso8211SubfieldFormatType.UnsignedInteger"/>,
    /// <see cref="Iso8211SubfieldFormatType.SignedInteger"/>), this is the number of bytes.
    /// </para>
    /// </remarks>
    public int Width { get; init; }

    /// <summary>
    /// Gets whether this subfield has a fixed width.
    /// </summary>
    public readonly bool IsFixedWidth => Width > 0;

    /// <summary>
    /// Gets whether this subfield has variable length (terminated by a delimiter).
    /// </summary>
    public readonly bool IsVariableLength => Width == 0;

    /// <summary>
    /// Gets the size in bytes of this subfield for binary types.
    /// </summary>
    /// <remarks>
    /// For binary types, this equals <see cref="Width"/>.
    /// For character-based types, this equals <see cref="Width"/> when fixed-length,
    /// or 0 when variable-length.
    /// </remarks>
    public readonly int ByteSize => FormatType switch
    {
        Iso8211SubfieldFormatType.UnsignedInteger => Width,
        Iso8211SubfieldFormatType.SignedInteger => Width,
        Iso8211SubfieldFormatType.BitString => Width,
        _ => Width
    };

    /// <inheritdoc/>
    public readonly bool Equals(Iso8211SubfieldFormat other) =>
        FormatType == other.FormatType && Width == other.Width;

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj) =>
        obj is Iso8211SubfieldFormat other && Equals(other);

    /// <inheritdoc/>
    public override readonly int GetHashCode() =>
        HashCode.Combine(FormatType, Width);

    /// <inheritdoc/>
    public override readonly string ToString() => FormatType switch
    {
        Iso8211SubfieldFormatType.CharacterData => Width > 0 ? $"A({Width})" : "A",
        Iso8211SubfieldFormatType.Integer => Width > 0 ? $"I({Width})" : "I",
        Iso8211SubfieldFormatType.Real => Width > 0 ? $"R({Width})" : "R",
        Iso8211SubfieldFormatType.UnsignedInteger => $"b1{Width}",
        Iso8211SubfieldFormatType.SignedInteger => $"b2{Width}",
        Iso8211SubfieldFormatType.BitString => $"B({Width * 8})",
        _ => $"?({Width})"
    };

    /// <summary>
    /// Determines whether two <see cref="Iso8211SubfieldFormat"/> values are equal.
    /// </summary>
    public static bool operator ==(Iso8211SubfieldFormat left, Iso8211SubfieldFormat right) =>
        left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Iso8211SubfieldFormat"/> values are not equal.
    /// </summary>
    public static bool operator !=(Iso8211SubfieldFormat left, Iso8211SubfieldFormat right) =>
        !left.Equals(right);
}

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

/// <summary>
/// Specifies the format type of an ISO 8211 subfield.
/// </summary>
public enum Iso8211SubfieldFormatType : byte
{
    /// <summary>
    /// Character (alphanumeric) data. Corresponds to format code <c>A</c>.
    /// </summary>
    CharacterData = 0,

    /// <summary>
    /// Integer data encoded as ASCII digits. Corresponds to format code <c>I</c>.
    /// </summary>
    Integer = 1,

    /// <summary>
    /// Real (floating-point) data encoded as ASCII. Corresponds to format code <c>R</c>.
    /// </summary>
    Real = 2,

    /// <summary>
    /// Unsigned binary integer data. Corresponds to format code <c>b1x</c> where x is the byte width.
    /// </summary>
    UnsignedInteger = 3,

    /// <summary>
    /// Signed binary integer data. Corresponds to format code <c>b2x</c> where x is the byte width.
    /// </summary>
    SignedInteger = 4,

    /// <summary>
    /// Bit string data. Corresponds to format code <c>B(n)</c> where n is the width in bits.
    /// The <see cref="Iso8211SubfieldFormat.Width"/> is stored in bytes (n / 8).
    /// </summary>
    BitString = 5
}
