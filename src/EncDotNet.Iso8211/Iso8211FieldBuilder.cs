using System.Collections.Immutable;

namespace EncDotNet.Iso8211;

/// <summary>
/// Builds an <see cref="Iso8211Field"/> by encoding subfield values according to a field definition.
/// </summary>
/// <remarks>
/// <para>
/// The builder encodes subfields in declaration order and, when the parent field defines a
/// repeating subfield group, wraps back to the start of the group — mirroring the parsing
/// behaviour of <see cref="Iso8211FieldReader"/>. Unit terminators (<c>0x1F</c>) are inserted
/// after variable-length subfields when another subfield follows; the trailing field terminator
/// (<c>0x1E</c>) is added by <see cref="Iso8211RecordWriter"/> when the record is written.
/// </para>
/// </remarks>
public sealed class Iso8211FieldBuilder
{
    private readonly string _tag;
    private readonly ImmutableArray<Iso8211SubfieldFormat> _formats;
    private readonly int _repeatingStartIndex;
    private readonly Iso8211WriterOptions _options;
    private readonly List<Segment> _segments = new();
    private int _index;

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso8211FieldBuilder"/> class from a field definition.
    /// </summary>
    /// <param name="definition">The field definition describing the field tag and subfield formats.</param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    public Iso8211FieldBuilder(Iso8211FieldDefinition definition, Iso8211WriterOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _tag = definition.Tag;
        _formats = definition.SubfieldDefinitions.Select(s => s.Format).ToImmutableArray();
        _repeatingStartIndex = definition.RepeatingSubfieldStartIndex;
        _options = options ?? Iso8211WriterOptions.Default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Iso8211FieldBuilder"/> class from explicit subfield formats.
    /// </summary>
    /// <param name="tag">The field tag.</param>
    /// <param name="formats">The subfield formats, in declaration order.</param>
    /// <param name="repeatingStartIndex">
    /// The index at which the repeating subfield group begins, or <c>-1</c> for no repeating group.
    /// </param>
    /// <param name="options">Optional writer options. Defaults to <see cref="Iso8211WriterOptions.Default"/>.</param>
    public Iso8211FieldBuilder(
        string tag,
        IEnumerable<Iso8211SubfieldFormat> formats,
        int repeatingStartIndex = -1,
        Iso8211WriterOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(formats);
        _tag = tag;
        _formats = formats.ToImmutableArray();
        _repeatingStartIndex = repeatingStartIndex;
        _options = options ?? Iso8211WriterOptions.Default;
    }

    /// <summary>
    /// Gets the field tag being built.
    /// </summary>
    public string Tag => _tag;

    /// <summary>
    /// Encodes a single subfield value using the next subfield format in sequence.
    /// </summary>
    /// <param name="value">The value to encode.</param>
    /// <returns>This builder, for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when there are no more subfield formats to consume and the field has no repeating group.
    /// </exception>
    public Iso8211FieldBuilder AddSubfield(object? value)
    {
        if (_formats.IsDefaultOrEmpty || _index >= _formats.Length)
        {
            throw new InvalidOperationException(
                "No subfield format is available for the next value. The field has no (further) subfield definitions.");
        }

        var format = _formats[_index];
        var bytes = Iso8211SubfieldEncoder.Encode(value, format, _options);
        _segments.Add(new Segment(bytes, !format.IsFixedWidth));

        _index++;
        if (_index >= _formats.Length && _repeatingStartIndex >= 0)
        {
            _index = _repeatingStartIndex;
        }

        return this;
    }

    /// <summary>
    /// Encodes multiple subfield values in sequence.
    /// </summary>
    /// <param name="values">The values to encode, in declaration order.</param>
    /// <returns>This builder, for chaining.</returns>
    public Iso8211FieldBuilder AddSubfields(params object?[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        foreach (var value in values)
        {
            AddSubfield(value);
        }
        return this;
    }

    /// <summary>
    /// Appends raw, pre-encoded bytes to the field data as a single variable-length segment.
    /// </summary>
    /// <param name="bytes">The raw bytes to append.</param>
    /// <param name="isVariableLength">
    /// Whether the appended segment should be followed by a unit terminator when another segment
    /// follows. Defaults to <c>true</c>.
    /// </param>
    /// <returns>This builder, for chaining.</returns>
    public Iso8211FieldBuilder AddRaw(byte[] bytes, bool isVariableLength = true)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        _segments.Add(new Segment(bytes, isVariableLength));
        return this;
    }

    /// <summary>
    /// Builds the <see cref="Iso8211Field"/> with the encoded subfield data.
    /// </summary>
    /// <returns>The built field. The data excludes the trailing field terminator.</returns>
    public Iso8211Field Build()
    {
        var unitTerminator = UnitTerminatorBytes(_options);
        var data = new List<byte>();

        for (int i = 0; i < _segments.Count; i++)
        {
            data.AddRange(_segments[i].Bytes);

            var isLast = i == _segments.Count - 1;
            if (!isLast && _segments[i].IsVariableLength)
            {
                data.AddRange(unitTerminator);
            }
        }

        return new Iso8211Field
        {
            Tag = _tag,
            Data = data.ToArray()
        };
    }

    private static byte[] UnitTerminatorBytes(Iso8211WriterOptions options) =>
        options.TerminatorWidth >= 2 ? new byte[] { 0x1F, 0x00 } : new byte[] { 0x1F };

    private readonly struct Segment
    {
        public Segment(byte[] bytes, bool isVariableLength)
        {
            Bytes = bytes;
            IsVariableLength = isVariableLength;
        }

        public byte[] Bytes { get; }

        public bool IsVariableLength { get; }
    }
}
