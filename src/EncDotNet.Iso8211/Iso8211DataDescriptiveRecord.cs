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
