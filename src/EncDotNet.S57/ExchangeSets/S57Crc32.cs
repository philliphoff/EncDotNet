namespace EncDotNet.S57.ExchangeSets;

/// <summary>
/// Computes the 32-bit cyclic redundancy check (CRC-32) used by S-57 exchange sets.
/// </summary>
/// <remarks>
/// <para>
/// S-57 Edition 3.1 Part 3, clause 3.4 defers the choice of CRC algorithm to the relevant
/// product specification. The ENC product specification (Appendix B.1) uses the ubiquitous
/// CRC-32 employed by the ZIP file format and Ethernet: the reflected polynomial
/// <c>0xEDB88320</c> (i.e. <c>0x04C11DB7</c> reversed), an initial value of all ones, and a
/// final XOR of all ones. This is the same algorithm exposed by <c>System.IO.Hashing.Crc32</c>
/// and <c>zlib</c>.
/// </para>
/// <para>
/// Implemented internally to avoid adding a package dependency for a single, well-known
/// table-based computation.
/// </para>
/// </remarks>
internal static class S57Crc32
{
    private const uint Polynomial = 0xEDB88320u;

    private static readonly uint[] Table = BuildTable();

    private static uint[] BuildTable()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ Polynomial : crc >> 1;
            }

            table[i] = crc;
        }

        return table;
    }

    /// <summary>
    /// Computes the CRC-32 of the bytes read from <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream to read to completion.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The computed CRC-32 value.</returns>
    public static async Task<uint> ComputeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        uint crc = 0xFFFFFFFFu;
        var buffer = new byte[81920];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            for (int i = 0; i < bytesRead; i++)
            {
                crc = (crc >> 8) ^ Table[(crc ^ buffer[i]) & 0xFF];
            }
        }

        return crc ^ 0xFFFFFFFFu;
    }

    /// <summary>
    /// Computes the CRC-32 of the supplied bytes.
    /// </summary>
    /// <param name="data">The data to checksum.</param>
    /// <returns>The computed CRC-32 value.</returns>
    public static uint Compute(ReadOnlySpan<byte> data)
    {
        uint crc = 0xFFFFFFFFu;
        foreach (byte b in data)
        {
            crc = (crc >> 8) ^ Table[(crc ^ b) & 0xFF];
        }

        return crc ^ 0xFFFFFFFFu;
    }

    /// <summary>
    /// Formats a CRC-32 value as the 8-character, upper-case hexadecimal string used in the
    /// CATD <c>CRCS</c> subfield (most-significant byte first).
    /// </summary>
    /// <param name="crc">The CRC-32 value.</param>
    /// <returns>An 8-character upper-case hexadecimal string.</returns>
    public static string Format(uint crc) => crc.ToString("X8");
}
