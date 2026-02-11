using System.Collections.Immutable;
using System.Globalization;
using EncDotNet.Iso8211;

namespace EncDotNet.Enc.Catalogs;

/// <summary>
/// Provides methods to read S-57 CATALOG.031 files.
/// </summary>
/// <remarks>
/// <para>
/// The CATALOG.031 file is an ISO 8211 document containing CATD (Catalogue Directory)
/// records that describe the chart files in an ENC exchange set.
/// </para>
/// <para>
/// This reader parses the ISO 8211 encoded catalog and constructs an <see cref="S57Catalog"/>
/// containing the catalog entries.
/// </para>
/// </remarks>
public static class S57CatalogReader
{
    /// <summary>
    /// Reads an S-57 catalog from a byte array.
    /// </summary>
    /// <param name="data">The catalog data to read.</param>
    /// <returns>The parsed catalog.</returns>
    public static S57Catalog Read(byte[] data)
    {
        var iso8211Document = Iso8211DocumentReader.Read(data);
        return ParseCatalog(iso8211Document);
    }

    /// <summary>
    /// Reads an S-57 catalog from a span of bytes.
    /// </summary>
    /// <param name="data">The catalog data to read.</param>
    /// <returns>The parsed catalog.</returns>
    public static S57Catalog Read(ReadOnlySpan<byte> data)
    {
        var iso8211Document = Iso8211DocumentReader.Read(data);
        return ParseCatalog(iso8211Document);
    }

    /// <summary>
    /// Reads an S-57 catalog from a file.
    /// </summary>
    /// <param name="path">The path to the CATALOG.031 file.</param>
    /// <returns>The parsed catalog.</returns>
    public static S57Catalog ReadFromFile(string path)
    {
        var iso8211Document = Iso8211DocumentReader.ReadFromFile(path);
        return ParseCatalog(iso8211Document);
    }

    /// <summary>
    /// Asynchronously reads an S-57 catalog from a file.
    /// </summary>
    /// <param name="path">The path to the CATALOG.031 file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public static async Task<S57Catalog> ReadFromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var iso8211Document = await Iso8211DocumentReader.ReadFromFileAsync(path, cancellationToken).ConfigureAwait(false);
        return ParseCatalog(iso8211Document);
    }

    /// <summary>
    /// Reads an S-57 catalog from a stream.
    /// </summary>
    /// <param name="stream">The stream containing catalog data.</param>
    /// <returns>The parsed catalog.</returns>
    public static S57Catalog Read(Stream stream)
    {
        var iso8211Document = Iso8211DocumentReader.Read(stream);
        return ParseCatalog(iso8211Document);
    }

    /// <summary>
    /// Asynchronously reads an S-57 catalog from a stream.
    /// </summary>
    /// <param name="stream">The stream containing catalog data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public static async Task<S57Catalog> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var iso8211Document = await Iso8211DocumentReader.ReadAsync(stream, cancellationToken).ConfigureAwait(false);
        return ParseCatalog(iso8211Document);
    }

    private static S57Catalog ParseCatalog(Iso8211Document iso8211Document)
    {
        var ddr = iso8211Document.DataDescriptiveRecord is not null
            ? Iso8211DataDescriptiveRecordReader.Read(iso8211Document.DataDescriptiveRecord)
            : null;

        var entries = ImmutableArray.CreateBuilder<S57CatalogEntry>();

        foreach (var record in iso8211Document.DataRecords)
        {
            var catdField = record.GetFieldByTag(S57FieldTags.CATD);
            if (catdField == null)
            {
                continue;
            }

            var entry = ParseCatalogEntry(catdField, ddr);
            if (entry != null)
            {
                entries.Add(entry);
            }
        }

        return new S57Catalog
        {
            Entries = entries.ToImmutable()
        };
    }

    private static S57CatalogEntry? ParseCatalogEntry(Iso8211Field catdField, Iso8211DataDescriptiveRecord? ddr)
    {
        var fieldDef = ddr?.GetFieldDefinition(S57FieldTags.CATD);
        if (fieldDef == null)
        {
            return null;
        }

        var reader = new Iso8211FieldReader(fieldDef, catdField.Data);

        var rcnm = reader.GetSubfield<string>(S57SubfieldNames.RCNM);
        var rcid = reader.GetSubfield<uint>(S57SubfieldNames.RCID);
        var file = reader.GetSubfield<string>(S57SubfieldNames.FILE);
        var lfil = reader.GetSubfield<string>(S57SubfieldNames.LFIL);
        var volm = reader.GetSubfield<string>(S57SubfieldNames.VOLM);
        var impl = reader.GetSubfield<string>(S57SubfieldNames.IMPL);

        var slat = ParseOptionalDouble(reader, S57SubfieldNames.SLAT);
        var wlon = ParseOptionalDouble(reader, S57SubfieldNames.WLON);
        var nlat = ParseOptionalDouble(reader, S57SubfieldNames.NLAT);
        var elon = ParseOptionalDouble(reader, S57SubfieldNames.ELON);

        reader.TryGetSubfield<string>(S57SubfieldNames.CRCS, out var crcs);
        reader.TryGetSubfield<string>(S57SubfieldNames.COMT, out var comt);

        return new S57CatalogEntry
        {
            RecordName = rcnm,
            RecordId = rcid,
            FileName = file,
            LongFileName = lfil,
            Volume = volm,
            Implementation = impl,
            SouthernmostLatitude = slat,
            WesternmostLongitude = wlon,
            NorthernmostLatitude = nlat,
            EasternmostLongitude = elon,
            CrcChecksum = crcs ?? "",
            Comment = comt ?? ""
        };
    }

    private static double? ParseOptionalDouble(Iso8211FieldReader reader, string subfieldName)
    {
        if (reader.TryGetSubfield<string>(subfieldName, out var value)
            && !string.IsNullOrEmpty(value)
            && double.TryParse(value, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return null;
    }
}
