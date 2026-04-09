namespace EncDotNet.Iso8211;

/// <summary>
/// Represents a group of subfields from a repeating subfield group.
/// </summary>
/// <remarks>
/// This class provides access to the values of subfields within a single repetition
/// of a repeating subfield group.
/// </remarks>
public sealed class Iso8211SubfieldGroup
{
    private readonly Iso8211FieldReader _reader;
    private readonly int _startIndex;
    private readonly int _length;
    private readonly int _groupIndex;

    internal Iso8211SubfieldGroup(Iso8211FieldReader reader, int startIndex, int length, int groupIndex)
    {
        _reader = reader;
        _startIndex = startIndex;
        _length = length;
        _groupIndex = groupIndex;
    }

    /// <summary>
    /// Gets the zero-based index of this group within the field's repeating groups.
    /// </summary>
    public int Index => _groupIndex;

    /// <summary>
    /// Gets the number of subfields in this group.
    /// </summary>
    public int Count => _length;

    /// <summary>
    /// Gets the value of a subfield within this group by name.
    /// </summary>
    /// <typeparam name="T">The type to convert the subfield value to.</typeparam>
    /// <param name="name">The name of the subfield to retrieve.</param>
    /// <returns>The subfield value converted to type <typeparamref name="T"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no subfield with the specified name exists in this group.
    /// </exception>
    public T GetSubfield<T>(string name)
    {
        var subfieldDef = _reader.FieldDefinition.GetSubfieldDefinition(name);
        if (subfieldDef is null)
        {
            throw new KeyNotFoundException($"Subfield '{name}' not found in field definition.");
        }

        var groupSubfieldIndex = subfieldDef.Index;
        if (_reader.FieldDefinition.HasRepeatingGroup)
        {
            groupSubfieldIndex = subfieldDef.Index - _reader.FieldDefinition.RepeatingSubfieldStartIndex;
        }

        if (groupSubfieldIndex >= 0 && groupSubfieldIndex < _length)
        {
            return _reader.GetSubfieldAt<T>(_startIndex + groupSubfieldIndex);
        }

        throw new KeyNotFoundException($"Subfield '{name}' not found in group {_groupIndex}.");
    }

    /// <summary>
    /// Gets the raw bytes of a subfield within this group by name.
    /// </summary>
    /// <param name="name">The name of the subfield to retrieve.</param>
    /// <returns>A span containing the raw subfield data.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no subfield with the specified name exists in this group.
    /// </exception>
    public ReadOnlySpan<byte> GetSubfieldBytes(string name)
    {
        var subfieldDef = _reader.FieldDefinition.GetSubfieldDefinition(name);
        if (subfieldDef is null)
        {
            throw new KeyNotFoundException($"Subfield '{name}' not found in field definition.");
        }

        var groupSubfieldIndex = subfieldDef.Index;
        if (_reader.FieldDefinition.HasRepeatingGroup)
        {
            groupSubfieldIndex = subfieldDef.Index - _reader.FieldDefinition.RepeatingSubfieldStartIndex;
        }

        if (groupSubfieldIndex >= 0 && groupSubfieldIndex < _length)
        {
            return _reader.GetSubfieldBytesAt(_startIndex + groupSubfieldIndex);
        }

        throw new KeyNotFoundException($"Subfield '{name}' not found in group {_groupIndex}.");
    }

    /// <summary>
    /// Gets the value of a subfield within this group by its position.
    /// </summary>
    /// <typeparam name="T">The type to convert the subfield value to.</typeparam>
    /// <param name="index">The zero-based index of the subfield within this group.</param>
    /// <returns>The subfield value converted to type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="index"/> is outside the valid range.
    /// </exception>
    public T GetSubfieldAt<T>(int index)
    {
        if (index < 0 || index >= _length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return _reader.GetSubfieldAt<T>(_startIndex + index);
    }
}
