using System.Runtime.CompilerServices;

namespace EncDotNet.Iso8211;

/// <summary>
/// The structure of an <see cref="Iso8211FieldDefinition"/>, worked out once and shared by
/// every <see cref="Iso8211FieldReader"/> that reads a field with that definition.
/// </summary>
/// <remarks>
/// <para>
/// Layouts are cached per definition instance in a <see cref="ConditionalWeakTable{TKey, TValue}"/>
/// rather than stored on the definition, because <see cref="Iso8211FieldDefinition"/> is a record:
/// a cached member would take part in its generated equality, and a <c>with</c> expression would
/// copy a stale layout onto a definition whose subfields had changed. Definitions are treated as
/// immutable once a reader has used them.
/// </para>
/// <para>
/// When every subfield has a fixed width, as in coordinate and pointer fields such as SG2D, SG3D,
/// FSPT and VRPT, the byte offset of any subfield instance follows from its position alone, so
/// readers don't need a per-instance offset table.
/// </para>
/// </remarks>
internal sealed class Iso8211FieldLayout
{
    private static readonly ConditionalWeakTable<Iso8211FieldDefinition, Iso8211FieldLayout> Cache = new();

    private readonly string[] _names;

    private Iso8211FieldLayout(Iso8211FieldDefinition definition)
    {
        var subfields = definition.SubfieldDefinitions;
        Count = subfields.Count;
        _names = new string[Count];
        Formats = new Iso8211SubfieldFormat[Count];
        Offsets = new int[Count];

        for (var i = 0; i < Count; i++)
        {
            _names[i] = subfields[i].Name;
            Formats[i] = subfields[i].Format;
        }

        RepeatStart = definition.RepeatingSubfieldStartIndex;
        HasRepeatingGroup = RepeatStart >= 0;
        PrefixCount = HasRepeatingGroup ? RepeatStart : Count;
        RepeatCount = HasRepeatingGroup ? Count - RepeatStart : 0;

        // A repeating group that starts past the last subfield is malformed; leave it to the
        // general path, which reports it as it always has.
        IsFixedWidth = Count > 0 && (!HasRepeatingGroup || RepeatCount > 0);
        for (var i = 0; i < Count && IsFixedWidth; i++)
        {
            IsFixedWidth = Formats[i].IsFixedWidth;
        }

        if (IsFixedWidth)
        {
            // Offsets of the leading subfields from the start of the field, and of the
            // repeating subfields from the start of their group.
            var offset = 0;
            for (var i = 0; i < PrefixCount; i++)
            {
                Offsets[i] = offset;
                offset += Formats[i].Width;
            }

            PrefixBytes = offset;
            offset = 0;
            for (var i = PrefixCount; i < Count; i++)
            {
                Offsets[i] = offset;
                offset += Formats[i].Width;
            }

            Stride = offset;
        }
    }

    /// <summary>Gets the number of subfield definitions.</summary>
    public int Count { get; }

    /// <summary>Gets each subfield's format, by definition position.</summary>
    public Iso8211SubfieldFormat[] Formats { get; }

    /// <summary>Gets whether the field has a repeating subfield group.</summary>
    public bool HasRepeatingGroup { get; }

    /// <summary>Gets the position of the first repeating subfield, or <c>-1</c>.</summary>
    public int RepeatStart { get; }

    /// <summary>Gets the number of subfields before the repeating group (all of them if none repeat).</summary>
    public int PrefixCount { get; }

    /// <summary>Gets the number of subfields in each repetition of the group.</summary>
    public int RepeatCount { get; }

    /// <summary>Gets whether every subfield has a fixed width.</summary>
    public bool IsFixedWidth { get; }

    /// <summary>
    /// Gets each subfield's byte offset when <see cref="IsFixedWidth"/>: from the start of the
    /// field for leading subfields, and from the start of the group for repeating ones.
    /// </summary>
    public int[] Offsets { get; }

    /// <summary>Gets the total width of the leading subfields when <see cref="IsFixedWidth"/>.</summary>
    public int PrefixBytes { get; }

    /// <summary>Gets the width of one repetition of the group when <see cref="IsFixedWidth"/>.</summary>
    public int Stride { get; }

    /// <summary>
    /// Gets the layout for a field definition, building it on first use.
    /// </summary>
    public static Iso8211FieldLayout For(Iso8211FieldDefinition definition) =>
        Cache.GetValue(definition, static d => new Iso8211FieldLayout(d));

    /// <summary>
    /// Gets the position of the first subfield definition with the given name, or <c>-1</c>.
    /// </summary>
    /// <remarks>
    /// A field has a handful of four-character names, so a loop is as fast as a dictionary and
    /// allocates nothing.
    /// </remarks>
    public int IndexOf(string name)
    {
        for (var i = 0; i < _names.Length; i++)
        {
            if (string.Equals(_names[i], name, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }
}
