using System.Collections.Immutable;

namespace EncDotNet.Iso8211;

/// <summary>
/// Represents a single field definition from the DDR.
/// </summary>
/// <remarks>
/// A field definition describes the structure and format of a field that appears in
/// data records. It includes the field tag, name, data structure type, data type,
/// and the definitions of all subfields within the field.
/// </remarks>
public sealed record Iso8211FieldDefinition
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
    public IReadOnlyList<Iso8211SubfieldDefinition> SubfieldDefinitions { get; init; } = [];

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
