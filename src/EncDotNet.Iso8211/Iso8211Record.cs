using System.Collections.Immutable;

namespace EncDotNet.Iso8211;

/// <summary>
/// Represents a single ISO 8211 record with its leader, directory, and fields.
/// </summary>
public sealed record Iso8211Record
{
    /// <summary>
    /// Gets the leader information for this record.
    /// </summary>
    public Iso8211RecordLeader Leader { get; init; } = default!;

    /// <summary>
    /// Gets the directory entries for this record.
    /// </summary>
    public ImmutableArray<Iso8211DirectoryEntry> Directory { get; init; }

    /// <summary>
    /// Gets the fields contained in this record.
    /// </summary>
    public ImmutableArray<Iso8211Field> Fields { get; init; }

    /// <summary>
    /// Gets whether this record is a Data Descriptive Record (DDR).
    /// </summary>
    public bool IsDataDescriptiveRecord => Leader.LeaderIdentifier == 'L';

    /// <summary>
    /// Gets a field by its tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>The field with the specified tag, or null if not found.</returns>
    public Iso8211Field? GetFieldByTag(string tag) => Fields.FirstOrDefault(f => f.Tag == tag);

    /// <summary>
    /// Gets all fields with the specified tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>All fields with the specified tag.</returns>
    public IEnumerable<Iso8211Field> GetFieldsByTag(string tag) => Fields.Where(f => f.Tag == tag);
}
