using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace EncDotNet.S57.Aml;

/// <summary>
/// Represents the structured metadata that NATO Additional Military Layers (AML) cells carry in
/// the data set identification comment (<see cref="S57DataSetIdentification.Comment"/>).
/// </summary>
/// <remarks>
/// <para>
/// An AML <c>DSID</c> <c>COMT</c> is a list of <c>key=value</c> pairs separated by semicolons,
/// for example <c>title=Wrecks; IDO Status = Final; Protective Marking = UNCLASSIFIED; Owner
/// Authority = UKHO</c>, optionally followed by a caveat. Keys are matched without regard to
/// case or white space, so <c>IDO Status</c>, <c>ido status</c> and <c>IDOStatus</c> are the same
/// key. Values are trimmed and may contain <c>=</c>; they can't contain <c>;</c>.
/// </para>
/// <para>
/// The well-known keys are exposed as properties, which are <see langword="null"/> when the
/// comment doesn't have the key. Every pair, including keys this type doesn't recognise, is
/// available from <see cref="Entries"/>.
/// </para>
/// </remarks>
public sealed class AmlDataSetMetadata
{
    private const string TitleKey = "title";
    private const string IdoStatusKey = "idostatus";
    private const string ProtectiveMarkingKey = "protectivemarking";
    private const string OwnerAuthorityKey = "ownerauthority";
    private const string CaveatKey = "caveat";

    private AmlDataSetMetadata(IReadOnlyDictionary<string, string> entries, Dictionary<string, string> normalized)
    {
        Entries = entries;
        Title = normalized.GetValueOrDefault(TitleKey);
        IdoStatus = normalized.GetValueOrDefault(IdoStatusKey);
        ProtectiveMarking = normalized.GetValueOrDefault(ProtectiveMarkingKey);
        OwnerAuthority = normalized.GetValueOrDefault(OwnerAuthorityKey);
        Caveat = normalized.GetValueOrDefault(CaveatKey);
    }

    /// <summary>Gets the data set title (<c>title</c>), or <see langword="null"/> if absent.</summary>
    public string? Title { get; }

    /// <summary>
    /// Gets the Information and Data Object (IDO) status (<c>IDO Status</c>), or
    /// <see langword="null"/> if absent.
    /// </summary>
    public string? IdoStatus { get; }

    /// <summary>
    /// Gets the security classification (<c>Protective Marking</c>), or <see langword="null"/> if
    /// absent.
    /// </summary>
    public string? ProtectiveMarking { get; }

    /// <summary>
    /// Gets the authority that owns the data (<c>Owner Authority</c>), or <see langword="null"/>
    /// if absent.
    /// </summary>
    public string? OwnerAuthority { get; }

    /// <summary>
    /// Gets the handling caveat (<c>Caveat</c>), or <see langword="null"/> if absent.
    /// </summary>
    public string? Caveat { get; }

    /// <summary>
    /// Gets every key/value pair in the comment, keyed by the trimmed key as written. Lookups
    /// ignore case. When a key appears more than once, the first value wins.
    /// </summary>
    public IReadOnlyDictionary<string, string> Entries { get; }

    /// <summary>
    /// Parses an AML data set identification comment.
    /// </summary>
    /// <param name="comment">The <c>DSID</c> <c>COMT</c> text.</param>
    /// <returns>The metadata the comment holds.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="comment"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">
    /// <paramref name="comment"/> isn't an AML comment; see <see cref="TryParse(string?, out AmlDataSetMetadata?)"/>.
    /// </exception>
    public static AmlDataSetMetadata Parse(string comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        return TryParse(comment, out var metadata)
            ? metadata
            : throw new FormatException("The comment isn't a list of AML 'key=value' pairs separated by semicolons.");
    }

    /// <summary>
    /// Tries to parse an AML data set identification comment.
    /// </summary>
    /// <param name="comment">The <c>DSID</c> <c>COMT</c> text.</param>
    /// <param name="metadata">
    /// When this method returns <see langword="true"/>, the metadata the comment holds; otherwise,
    /// <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if every non-empty, semicolon-separated part of
    /// <paramref name="comment"/> is a <c>key=value</c> pair with a non-empty key and at least one
    /// key is a well-known AML key; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(string? comment, [NotNullWhen(true)] out AmlDataSetMetadata? metadata)
    {
        metadata = null;
        if (string.IsNullOrWhiteSpace(comment))
        {
            return false;
        }

        var entries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var normalized = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var part in comment.Split(';'))
        {
            if (string.IsNullOrWhiteSpace(part))
            {
                continue;
            }

            var separator = part.IndexOf('=');
            if (separator < 0)
            {
                return false;
            }

            var key = part[..separator].Trim();
            if (key.Length == 0)
            {
                return false;
            }

            var value = part[(separator + 1)..].Trim();
            entries.TryAdd(key, value);
            normalized.TryAdd(NormalizeKey(key), value);
        }

        if (!normalized.ContainsKey(TitleKey)
            && !normalized.ContainsKey(IdoStatusKey)
            && !normalized.ContainsKey(ProtectiveMarkingKey)
            && !normalized.ContainsKey(OwnerAuthorityKey)
            && !normalized.ContainsKey(CaveatKey))
        {
            return false;
        }

        metadata = new AmlDataSetMetadata(new ReadOnlyDictionary<string, string>(entries), normalized);
        return true;
    }

    private static string NormalizeKey(string key) =>
        string.Concat(key.Where(c => !char.IsWhiteSpace(c))).ToLowerInvariant();
}
