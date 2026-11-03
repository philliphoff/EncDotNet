namespace EncDotNet.S57;

/// <summary>
/// Provides methods to choose the S-57 lexical levels (DSSI AALL and NALL) for character data.
/// </summary>
/// <remarks>
/// <para>
/// These methods make the same checks as <see cref="S57DocumentWriter"/>, so a level they pick
/// is one the writer can encode the values at (S-57 Edition 3.1, Part 3, clause 2.4).
/// </para>
/// <para>
/// UCS-2 covers the Basic Multilingual Plane only. A character outside it, which .NET holds as a
/// UTF-16 surrogate pair (an emoji, for example), can't be encoded at any lexical level, and an
/// unpaired surrogate can't either.
/// </para>
/// </remarks>
public static class S57LexicalLevels
{
    /// <summary>
    /// Gets whether a value can be encoded at a lexical level.
    /// </summary>
    /// <param name="value">The value. <see langword="null"/> is treated as an empty string.</param>
    /// <param name="level">The lexical level.</param>
    /// <returns>
    /// <see langword="true"/> if every character of <paramref name="value"/> is in the character
    /// set of <paramref name="level"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> isn't 0, 1 or 2.</exception>
    public static bool CanEncode(string? value, S57LexicalLevel level) =>
        S57FieldEncoder.CanEncode(value ?? string.Empty, S57FieldEncoder.ForLexicalLevel(level));

    /// <summary>
    /// Gets the lowest lexical level that can encode every one of a set of values.
    /// </summary>
    /// <param name="values">
    /// The values, such as the ATTF or NATF attribute values of a data set.
    /// <see langword="null"/> values are treated as empty strings.
    /// </param>
    /// <returns>
    /// <see cref="S57LexicalLevel.Ascii"/> if every value is ASCII (including when there are no
    /// values), <see cref="S57LexicalLevel.Latin1"/> if every value is ISO 8859-1, and otherwise
    /// <see cref="S57LexicalLevel.Ucs2"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// A value has a character outside the Basic Multilingual Plane (a surrogate pair) or an
    /// unpaired surrogate, which no lexical level can encode.
    /// </exception>
    public static S57LexicalLevel Lowest(IEnumerable<string?> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var level = S57LexicalLevel.Ascii;
        foreach (var value in values)
        {
            // Levels are nested (ASCII within ISO 8859-1 within UCS-2), so a value never moves the
            // level down, and each value is checked from the level reached so far.
            while (!CanEncode(value, level))
            {
                if (level == S57LexicalLevel.Ucs2)
                {
                    throw new ArgumentException(
                        $"The value '{value}' has a character outside the Basic Multilingual Plane, which no S-57 lexical level can encode (S-57 Part 3, clause 2.4).",
                        nameof(values));
                }

                level++;
            }
        }

        return level;
    }

    /// <summary>
    /// Returns a copy of a document whose DSSI lexical levels (AALL and NALL) are the lowest that
    /// can encode its feature records' ATTF and NATF values.
    /// </summary>
    /// <param name="document">The document. It must have a data set identification (DSID).</param>
    /// <returns>
    /// A copy of <paramref name="document"/> whose
    /// <see cref="S57DataSetIdentification.AttfLexicalLevel"/> and
    /// <see cref="S57DataSetIdentification.NatfLexicalLevel"/> are set from
    /// <see cref="Lowest"/>. The levels may be lower than the document's.
    /// </returns>
    /// <remarks>
    /// <see cref="S57DocumentWriter"/> writes the levels the document holds. Call this before
    /// writing a document built in memory to have the writer use the lowest levels instead.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="document"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// The document has no data set identification; an ATTF value needs UCS-2, which ATTF doesn't
    /// allow; or a value has a character that no lexical level can encode.
    /// </exception>
    public static S57Document WithLowestLevels(S57Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var dsid = document.DataSetIdentification
            ?? throw new ArgumentException("The document has no data set identification (DSID).", nameof(document));

        var attf = Lowest(document.FeatureRecords.SelectMany(f => f.Attributes).Select(a => a.Value));
        if (attf > S57LexicalLevel.Latin1)
        {
            throw new ArgumentException(
                "An ATTF value has a character outside ISO 8859-1; ATTF allows only lexical levels 0 or 1 (S-57 Part 3, clause 2.4).",
                nameof(document));
        }

        var natf = Lowest(document.FeatureRecords.SelectMany(f => f.NationalAttributes).Select(a => a.Value));

        return document with
        {
            DataSetIdentification = dsid with { AttfLexicalLevel = (int)attf, NatfLexicalLevel = (int)natf }
        };
    }
}
