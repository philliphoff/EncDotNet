using System.Globalization;
using EncDotNet.Iso8211;

namespace EncDotNet.S57.ExchangeSets;

/// <summary>
/// Provides methods to write an <see cref="S57Catalog"/> as an S-57 <c>CATALOG.031</c> file.
/// </summary>
/// <remarks>
/// <para>
/// The writer is the inverse of <see cref="S57CatalogReader"/>. It writes the catalogue in the
/// ASCII implementation that exchange sets use: a DDR, then one record per entry holding the
/// <c>0001</c> record identifier (numbered from 0) and a CATD field (S-57 Part 3, clause 7.8.1).
/// </para>
/// <para>
/// The output depends only on the catalogue, so writing the same catalogue always produces the
/// same bytes. Coordinates are written with six decimal places when that represents them
/// exactly, and otherwise with as many as they need; a missing coordinate is written empty.
/// An entry whose <see cref="S57CatalogEntry.RecordName"/> is empty is written as <c>CD</c>.
/// </para>
/// <para>
/// Use <see cref="ComputeCrc(ReadOnlySpan{byte})"/> to fill in <see cref="S57CatalogEntry.CrcChecksum"/>
/// for each file in the exchange set; the catalogue's own entry has none.
/// </para>
/// </remarks>
public static class S57CatalogWriter
{
    private const string RecordIdentifierTag = "0001";
    private const string CatalogueDirectoryRecordName = "CD";

    private static readonly Iso8211WriterOptions WriterOptions = new()
    {
        BlankDataRecordFieldControlLength = true
    };

    /// <summary>
    /// Writes a catalogue to a byte array.
    /// </summary>
    /// <param name="catalog">The catalogue to write.</param>
    /// <returns>The encoded <c>CATALOG.031</c> file.</returns>
    /// <exception cref="ArgumentException">An entry can't be encoded; the message names it.</exception>
    public static byte[] Write(S57Catalog catalog) =>
        Iso8211DocumentWriter.Write(Encode(catalog), WriterOptions);

    /// <summary>
    /// Writes a catalogue to a stream.
    /// </summary>
    /// <param name="stream">The destination stream.</param>
    /// <param name="catalog">The catalogue to write.</param>
    /// <exception cref="ArgumentException">An entry can't be encoded. Nothing is written to the stream.</exception>
    public static void Write(Stream stream, S57Catalog catalog)
    {
        ArgumentNullException.ThrowIfNull(stream);
        Iso8211DocumentWriter.Write(stream, Encode(catalog), WriterOptions);
    }

    /// <summary>
    /// Asynchronously writes a catalogue to a stream.
    /// </summary>
    /// <param name="stream">The destination stream.</param>
    /// <param name="catalog">The catalogue to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentException">An entry can't be encoded. Nothing is written to the stream.</exception>
    public static Task WriteAsync(Stream stream, S57Catalog catalog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return Iso8211DocumentWriter.WriteAsync(stream, Encode(catalog), WriterOptions, cancellationToken);
    }

    /// <summary>
    /// Writes a catalogue to a file.
    /// </summary>
    /// <param name="path">The destination file path, normally <c>CATALOG.031</c> at the exchange set root.</param>
    /// <param name="catalog">The catalogue to write.</param>
    /// <exception cref="ArgumentException">An entry can't be encoded. The file isn't created or changed.</exception>
    public static void WriteToFile(string path, S57Catalog catalog) =>
        Iso8211DocumentWriter.WriteToFile(path, Encode(catalog), WriterOptions);

    /// <summary>
    /// Asynchronously writes a catalogue to a file.
    /// </summary>
    /// <param name="path">The destination file path, normally <c>CATALOG.031</c> at the exchange set root.</param>
    /// <param name="catalog">The catalogue to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentException">An entry can't be encoded. The file isn't created or changed.</exception>
    public static Task WriteToFileAsync(string path, S57Catalog catalog, CancellationToken cancellationToken = default) =>
        Iso8211DocumentWriter.WriteToFileAsync(path, Encode(catalog), WriterOptions, cancellationToken);

    /// <summary>
    /// Computes the CRC of a file's contents, formatted for <see cref="S57CatalogEntry.CrcChecksum"/>.
    /// </summary>
    /// <param name="data">The file's contents.</param>
    /// <returns>The CRC-32 as eight upper-case hexadecimal digits, as the CATD CRCS subfield holds it.</returns>
    /// <remarks>
    /// This is the CRC-32 that <see cref="S57ExchangeSetVerifier"/> checks (S-57 Part 3,
    /// clause 3.4, and the ENC product specification).
    /// </remarks>
    public static string ComputeCrc(ReadOnlySpan<byte> data) => S57Crc32.Format(S57Crc32.Compute(data));

    /// <summary>
    /// Asynchronously computes the CRC of a stream's contents, formatted for
    /// <see cref="S57CatalogEntry.CrcChecksum"/>.
    /// </summary>
    /// <param name="stream">The stream to read to its end.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The CRC-32 as eight upper-case hexadecimal digits.</returns>
    public static async Task<string> ComputeCrcAsync(Stream stream, CancellationToken cancellationToken = default) =>
        S57Crc32.Format(await S57Crc32.ComputeAsync(stream, cancellationToken).ConfigureAwait(false));

    // Encodes the catalogue as ISO 8211 records, checking every entry before anything is written.
    private static Iso8211Document Encode(S57Catalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var encoder = new S57FieldEncoder();
        var records = new List<Iso8211Record>(catalog.Entries.Count + 1)
        {
            S57DataDescriptiveRecord.BuildCatalog()
        };

        for (var i = 0; i < catalog.Entries.Count; i++)
        {
            var entry = catalog.Entries[i];
            var context = $"{S57FieldTags.CATD} ({entry.RecordId}, {entry.FileName})";

            var builder = new Iso8211RecordBuilder(WriterOptions)
            {
                InterchangeLevel = ' ',
                LeaderIdentifier = 'D',
                InlineCodeExtensionIndicator = ' ',
                VersionNumber = ' ',
                ApplicationIndicator = ' ',
                ExtendedCharacterSetIndicator = "   ",
                SizeOfFieldLengthField = 5,
                SizeOfFieldPositionField = 5
            };

            builder.AddField(encoder.Begin(context).FixedInteger(i, 5, RecordIdentifierTag).ToField(RecordIdentifierTag));

            // S-57 Part 3, clause 7.8.1.1.
            encoder.Begin(context)
                .FixedText(entry.RecordName.Length == 0 ? CatalogueDirectoryRecordName : entry.RecordName, 2, S57SubfieldNames.RCNM)
                .FixedInteger(entry.RecordId, 10, S57SubfieldNames.RCID)
                .Text(entry.FileName, S57SubfieldNames.FILE, S57FieldEncoder.Ascii)
                .Text(entry.LongFileName, S57SubfieldNames.LFIL, S57FieldEncoder.Ascii)
                .Text(entry.Volume, S57SubfieldNames.VOLM, S57FieldEncoder.Ascii)
                .FixedText(entry.Implementation, 3, S57SubfieldNames.IMPL)
                .Text(FormatCoordinate(entry.SouthernmostLatitude), S57SubfieldNames.SLAT, S57FieldEncoder.Ascii)
                .Text(FormatCoordinate(entry.WesternmostLongitude), S57SubfieldNames.WLON, S57FieldEncoder.Ascii)
                .Text(FormatCoordinate(entry.NorthernmostLatitude), S57SubfieldNames.NLAT, S57FieldEncoder.Ascii)
                .Text(FormatCoordinate(entry.EasternmostLongitude), S57SubfieldNames.ELON, S57FieldEncoder.Ascii)
                .Text(entry.CrcChecksum, S57SubfieldNames.CRCS, S57FieldEncoder.Ascii)
                .Text(entry.Comment, S57SubfieldNames.COMT, S57FieldEncoder.Ascii);
            builder.AddField(encoder.ToField(S57FieldTags.CATD));

            records.Add(builder.Build());
        }

        return new Iso8211Document { Records = records };
    }

    // Six decimal places, the usual precision of catalogue bounds, when they hold the value
    // exactly; otherwise the shortest text that reads back as the same value.
    private static string FormatCoordinate(double? value)
    {
        if (value is not { } coordinate)
        {
            return string.Empty;
        }

        var fixedText = coordinate.ToString("0.000000", CultureInfo.InvariantCulture);
        return double.Parse(fixedText, CultureInfo.InvariantCulture) == coordinate
            ? fixedText
            : coordinate.ToString("R", CultureInfo.InvariantCulture);
    }
}
