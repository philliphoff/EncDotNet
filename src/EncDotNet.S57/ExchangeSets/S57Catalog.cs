using System.Collections.Immutable;

namespace EncDotNet.S57.ExchangeSets;

/// <summary>
/// Represents the contents of an S-57 CATALOG.031 file.
/// </summary>
/// <remarks>
/// <para>
/// The CATALOG.031 file is located at the root of an ENC exchange set and describes
/// the chart files and other assets contained within it. Each entry in the catalog
/// corresponds to a CATD (Catalogue Directory) record.
/// </para>
/// <para>
/// Use <see cref="S57CatalogReader"/> to parse a CATALOG.031 file into this type.
/// </para>
/// </remarks>
public sealed class S57Catalog
{
    /// <summary>Gets all catalog entries.</summary>
    public ImmutableArray<S57CatalogEntry> Entries { get; init; }
}
