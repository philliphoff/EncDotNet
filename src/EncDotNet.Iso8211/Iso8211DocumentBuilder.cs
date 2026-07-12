namespace EncDotNet.Iso8211;

/// <summary>
/// Builds an <see cref="Iso8211Document"/> from a sequence of records.
/// </summary>
/// <remarks>
/// This is a convenience aggregator over <see cref="Iso8211RecordBuilder"/>. The resulting
/// document can be serialized with <see cref="Iso8211DocumentWriter"/>.
/// </remarks>
public sealed class Iso8211DocumentBuilder
{
    private readonly List<Iso8211Record> _records = new();

    /// <summary>
    /// Adds a record to the document.
    /// </summary>
    /// <param name="record">The record to add.</param>
    /// <returns>This builder, for chaining.</returns>
    public Iso8211DocumentBuilder AddRecord(Iso8211Record record)
    {
        ArgumentNullException.ThrowIfNull(record);
        _records.Add(record);
        return this;
    }

    /// <summary>
    /// Builds a record with <see cref="Iso8211RecordBuilder"/> and adds it to the document.
    /// </summary>
    /// <param name="recordBuilder">The record builder to build and add.</param>
    /// <returns>This builder, for chaining.</returns>
    public Iso8211DocumentBuilder AddRecord(Iso8211RecordBuilder recordBuilder)
    {
        ArgumentNullException.ThrowIfNull(recordBuilder);
        _records.Add(recordBuilder.Build());
        return this;
    }

    /// <summary>
    /// Builds the <see cref="Iso8211Document"/>.
    /// </summary>
    /// <returns>The built document.</returns>
    public Iso8211Document Build() => new() { Records = new List<Iso8211Record>(_records) };
}
