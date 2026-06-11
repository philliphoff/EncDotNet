using System.Text;
using EncDotNet.S57.ExchangeSets;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for <see cref="S57ExchangeSetVerifier"/>, <see cref="S57Crc32"/>, and the
/// associated verification result model.
/// </summary>
public class S57ExchangeSetVerifierTests
{
    #region ISO 8211 catalog fixture helpers (mirrors S57CatalogReaderTests)

    private const byte UnitTerminator = 0x1F;
    private const byte FieldTerminator = 0x1E;

    private static byte[] CreateCatalogDocument(params byte[][] dataRecords)
    {
        var ddr = CreateCatalogDdr();
        var totalSize = ddr.Length + dataRecords.Sum(r => r.Length);
        var result = new byte[totalSize];
        var offset = 0;

        Array.Copy(ddr, 0, result, offset, ddr.Length);
        offset += ddr.Length;

        foreach (var record in dataRecords)
        {
            Array.Copy(record, 0, result, offset, record.Length);
            offset += record.Length;
        }

        return result;
    }

    private static byte[] CreateCatalogDdr()
    {
        var fields = new List<(string tag, byte[] data)>
        {
            ("0001", CreateDdrFieldData("", "", "()")),
            ("CATD", CreateDdrFieldData(
                "CATD",
                "RCNM!RCID!FILE!LFIL!VOLM!IMPL!SLAT!WLON!NLAT!ELON!CRCS!COMT",
                "(A,I,A,A,A,A,A,A,A,A,A,A)"))
        };

        return CreateDdrRecord(fields.ToArray());
    }

    private static byte[] CreateDdrFieldData(string fieldName, string subfieldDescriptors, string formatControls)
    {
        using var ms = new MemoryStream();
        ms.WriteByte((byte)'0');
        ms.WriteByte((byte)'6');

        var descriptors = string.IsNullOrEmpty(fieldName)
            ? subfieldDescriptors
            : (string.IsNullOrEmpty(subfieldDescriptors) ? fieldName : $"{fieldName}!{subfieldDescriptors}");

        if (!string.IsNullOrEmpty(descriptors))
        {
            ms.Write(Encoding.ASCII.GetBytes(descriptors));
        }

        ms.WriteByte(UnitTerminator);
        ms.Write(Encoding.ASCII.GetBytes(formatControls));
        ms.WriteByte(FieldTerminator);

        return ms.ToArray();
    }

    private static byte[] CreateDdrRecord((string tag, byte[] data)[] fields)
    {
        var directoryEntries = new List<byte[]>();
        var currentPosition = 0;

        foreach (var (tag, data) in fields)
        {
            var entry = Encoding.ASCII.GetBytes($"{tag}{data.Length:D3}{currentPosition:D3}");
            directoryEntries.Add(entry);
            currentPosition += data.Length;
        }

        var directorySize = directoryEntries.Sum(e => e.Length);
        var baseAddress = 24 + directorySize + 1;
        var totalFieldSize = fields.Sum(f => f.data.Length);
        var recordLength = baseAddress + totalFieldSize;

        var leader = Encoding.ASCII.GetBytes($"{recordLength:D5}3LE1 02{baseAddress:D5}   3304");

        var record = new byte[recordLength];
        var offset = 0;

        Array.Copy(leader, 0, record, offset, leader.Length);
        offset += leader.Length;

        foreach (var entry in directoryEntries)
        {
            Array.Copy(entry, 0, record, offset, entry.Length);
            offset += entry.Length;
        }

        record[offset++] = FieldTerminator;

        foreach (var (_, data) in fields)
        {
            Array.Copy(data, 0, record, offset, data.Length);
            offset += data.Length;
        }

        return record;
    }

    private static byte[] CreateCatdRecord(
        string rcnm = "CD",
        uint rcid = 1,
        string file = "US5WA51M.000",
        string lfil = "US5WA51M.000",
        string volm = "V01X01",
        string impl = "BIN",
        string? slat = null,
        string? wlon = null,
        string? nlat = null,
        string? elon = null,
        string crcs = "",
        string comt = "")
    {
        using var ms = new MemoryStream();

        WriteString(ms, rcnm);
        WriteString(ms, rcid.ToString());
        WriteString(ms, file);
        WriteString(ms, lfil);
        WriteString(ms, volm);
        WriteString(ms, impl);
        WriteString(ms, slat ?? "");
        WriteString(ms, wlon ?? "");
        WriteString(ms, nlat ?? "");
        WriteString(ms, elon ?? "");
        WriteString(ms, crcs);
        WriteString(ms, comt);
        ms.WriteByte(FieldTerminator);

        return CreateDataRecord("CATD", ms.ToArray());
    }

    private static byte[] CreateDataRecord(string tag, byte[] fieldData)
    {
        var directoryEntry = Encoding.ASCII.GetBytes($"{tag}{fieldData.Length:D3}000");
        var baseAddress = 24 + directoryEntry.Length + 1;
        var recordLength = baseAddress + fieldData.Length;

        var leader = Encoding.ASCII.GetBytes($"{recordLength:D5}3DE1 00{baseAddress:D5}   3304");

        var record = new byte[recordLength];
        var offset = 0;

        Array.Copy(leader, 0, record, offset, leader.Length);
        offset += leader.Length;

        Array.Copy(directoryEntry, 0, record, offset, directoryEntry.Length);
        offset += directoryEntry.Length;

        record[offset++] = FieldTerminator;

        Array.Copy(fieldData, 0, record, offset, fieldData.Length);

        return record;
    }

    private static void WriteString(MemoryStream ms, string value)
    {
        ms.Write(Encoding.ASCII.GetBytes(value));
        ms.WriteByte(UnitTerminator);
    }

    /// <summary>
    /// Creates a self-contained temporary exchange set directory with a CATALOG.031 describing
    /// the supplied (relative-path, content) cell files, computing each file's CRC and writing
    /// it to the CRCS subfield unless an override is provided.
    /// </summary>
    private static string CreateExchangeSet(
        IEnumerable<(string relativePath, byte[] content, string? crcOverride)> files)
    {
        var root = Path.Combine(Path.GetTempPath(), "s57-verify-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var records = new List<byte[]>();
        uint rcid = 1;

        foreach (var (relativePath, content, crcOverride) in files)
        {
            var fullPath = Path.Combine(root, Path.Combine(relativePath.Split('/')));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllBytes(fullPath, content);

            string crc = crcOverride ?? S57Crc32.Format(S57Crc32.Compute(content));
            records.Add(CreateCatdRecord(rcid: rcid++, file: relativePath, lfil: relativePath, crcs: crc));
        }

        File.WriteAllBytes(Path.Combine(root, "CATALOG.031"), CreateCatalogDocument(records.ToArray()));
        return root;
    }

    #endregion

    #region CRC32 known-answer tests

    [Fact]
    public void Crc32_KnownAnswer_MatchesIeeeVector()
    {
        // "123456789" → 0xCBF43926 is the canonical CRC-32/ISO-HDLC check value.
        var data = Encoding.ASCII.GetBytes("123456789");

        uint crc = S57Crc32.Compute(data);

        Assert.Equal(0xCBF43926u, crc);
        Assert.Equal("CBF43926", S57Crc32.Format(crc));
    }

    [Fact]
    public void Crc32_EmptyInput_IsZero()
    {
        Assert.Equal(0u, S57Crc32.Compute([]));
        Assert.Equal("00000000", S57Crc32.Format(0u));
    }

    #endregion

    #region Verifier tests

    [Fact]
    public async Task VerifyAsync_MatchingCrc_ReturnsOk()
    {
        var content = Encoding.ASCII.GetBytes("ENC CELL CONTENT");
        var root = CreateExchangeSet([("US5WA51M/US5WA51M.000", content, null)]);

        try
        {
            var catalog = S57CatalogReader.ReadFromFile(Path.Combine(root, "CATALOG.031"));
            var result = await new S57ExchangeSetVerifier().VerifyAsync(root, catalog);

            var file = Assert.Single(result.FileResults);
            Assert.Equal(S57VerificationOutcome.Ok, file.ChecksumOutcome);
            Assert.Equal(S57VerificationOutcome.NotSigned, file.SignatureOutcome);
            Assert.Equal(file.ExpectedCrc, file.ActualCrc);
            Assert.True(result.AllValid);
            Assert.True(result.IsUnsigned);
            Assert.False(result.HasChecksumMismatches);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task VerifyAsync_WrongCrc_ReturnsChecksumMismatch()
    {
        var content = Encoding.ASCII.GetBytes("ENC CELL CONTENT");
        var root = CreateExchangeSet([("US5WA51M/US5WA51M.000", content, "DEADBEEF")]);

        try
        {
            var catalog = S57CatalogReader.ReadFromFile(Path.Combine(root, "CATALOG.031"));
            var result = await new S57ExchangeSetVerifier().VerifyAsync(root, catalog);

            var file = Assert.Single(result.FileResults);
            Assert.Equal(S57VerificationOutcome.ChecksumMismatch, file.ChecksumOutcome);
            Assert.Equal("DEADBEEF", file.ExpectedCrc);
            Assert.NotNull(file.ActualCrc);
            Assert.NotEqual(file.ExpectedCrc, file.ActualCrc);
            Assert.False(result.AllValid);
            Assert.True(result.HasChecksumMismatches);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task VerifyAsync_EmptyCrc_ReturnsNoChecksum()
    {
        var content = Encoding.ASCII.GetBytes("CONTENT");
        var root = CreateExchangeSet([("US5WA51M/US5WA51M.000", content, "")]);

        try
        {
            var catalog = S57CatalogReader.ReadFromFile(Path.Combine(root, "CATALOG.031"));
            var result = await new S57ExchangeSetVerifier().VerifyAsync(root, catalog);

            var file = Assert.Single(result.FileResults);
            Assert.Equal(S57VerificationOutcome.NoChecksum, file.ChecksumOutcome);
            // A missing CRC is not a failure (CRCs are optional in S-57), so AllValid holds.
            Assert.True(result.AllValid);
            Assert.False(result.HasChecksumMismatches);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task VerifyAsync_MissingFile_ReturnsFileMissing()
    {
        var content = Encoding.ASCII.GetBytes("CONTENT");
        var root = CreateExchangeSet([("US5WA51M/US5WA51M.000", content, null)]);

        try
        {
            // Delete the cell file but leave the catalog (with its CRC) in place.
            File.Delete(Path.Combine(root, "US5WA51M", "US5WA51M.000"));

            var catalog = S57CatalogReader.ReadFromFile(Path.Combine(root, "CATALOG.031"));
            var result = await new S57ExchangeSetVerifier().VerifyAsync(root, catalog);

            var file = Assert.Single(result.FileResults);
            Assert.Equal(S57VerificationOutcome.FileMissing, file.ChecksumOutcome);
            Assert.True(result.HasMissingFiles);
            Assert.False(result.AllValid);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task VerifyAsync_MultipleFiles_ReportsEachIndependently()
    {
        var good = Encoding.ASCII.GetBytes("GOOD CELL");
        var bad = Encoding.ASCII.GetBytes("BAD CELL");
        var root = CreateExchangeSet(
        [
            ("US5WA51M/US5WA51M.000", good, null),
            ("US5WA52M/US5WA52M.000", bad, "00000001"),
        ]);

        try
        {
            var catalog = S57CatalogReader.ReadFromFile(Path.Combine(root, "CATALOG.031"));
            var result = await new S57ExchangeSetVerifier().VerifyAsync(root, catalog);

            Assert.Equal(2, result.FileResults.Length);
            Assert.Equal(S57VerificationOutcome.Ok, result.FileResults[0].ChecksumOutcome);
            Assert.Equal(S57VerificationOutcome.ChecksumMismatch, result.FileResults[1].ChecksumOutcome);
            Assert.True(result.HasChecksumMismatches);
            Assert.False(result.AllValid);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task VerifyAsync_LowercaseCrc_IsAcceptedCaseInsensitively()
    {
        var content = Encoding.ASCII.GetBytes("CASE INSENSITIVE");
        string crc = S57Crc32.Format(S57Crc32.Compute(content)).ToLowerInvariant();
        var root = CreateExchangeSet([("US5WA51M/US5WA51M.000", content, crc)]);

        try
        {
            var catalog = S57CatalogReader.ReadFromFile(Path.Combine(root, "CATALOG.031"));
            var result = await new S57ExchangeSetVerifier().VerifyAsync(root, catalog);

            Assert.Equal(S57VerificationOutcome.Ok, Assert.Single(result.FileResults).ChecksumOutcome);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task S57ExchangeSet_VerifyAsync_ReadsCatalogAndVerifies()
    {
        var content = Encoding.ASCII.GetBytes("BASE CELL");
        var root = CreateExchangeSet([("US5WA51M/US5WA51M.000", content, null)]);

        try
        {
            var exchangeSet = S57ExchangeSetReader.Read(root);
            var result = await exchangeSet.VerifyAsync(root);

            Assert.True(result.AllValid);
            Assert.Equal(S57VerificationOutcome.Ok, Assert.Single(result.FileResults).ChecksumOutcome);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task VerifyAsync_SignatureVerifierSupplied_OutcomeIsReported()
    {
        var content = Encoding.ASCII.GetBytes("SIGNED CELL");
        var root = CreateExchangeSet([("US5WA51M/US5WA51M.000", content, null)]);

        try
        {
            var catalog = S57CatalogReader.ReadFromFile(Path.Combine(root, "CATALOG.031"));
            var verifier = new S57ExchangeSetVerifier(new StubSignatureVerifier(S57VerificationOutcome.SignatureInvalid));
            var result = await verifier.VerifyAsync(root, catalog);

            var file = Assert.Single(result.FileResults);
            Assert.Equal(S57VerificationOutcome.Ok, file.ChecksumOutcome);
            Assert.Equal(S57VerificationOutcome.SignatureInvalid, file.SignatureOutcome);
            Assert.True(result.HasInvalidSignatures);
            Assert.False(result.IsUnsigned);
            Assert.False(result.AllValid);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private sealed class StubSignatureVerifier(S57VerificationOutcome outcome) : IS63SignatureVerifier
    {
        public Task<S57VerificationOutcome> VerifySignatureAsync(
            string filePath, S63TrustAnchorOptions trustAnchors, CancellationToken cancellationToken = default)
            => Task.FromResult(outcome);
    }

    #endregion
}
