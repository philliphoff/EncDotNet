namespace EncDotNet.S57;

/// <summary>
/// The exchange purpose codes of the DSID subfield EXPP (S-57 Edition 3.1, Part 3 §7.3.1.1).
/// </summary>
/// <remarks>
/// The values are the exact S-57 codes, so cast between this enum and
/// <see cref="S57DataSetIdentification.ExchangePurpose"/> in either direction. A code S-57 doesn't
/// define casts to an unnamed value rather than failing.
/// </remarks>
public enum S57ExchangePurpose
{
    /// <summary>The data set is new (<c>N</c>, code 1).</summary>
    New = 1,

    /// <summary>The data set is a revision to an existing one (<c>R</c>, code 2).</summary>
    Revision = 2
}
