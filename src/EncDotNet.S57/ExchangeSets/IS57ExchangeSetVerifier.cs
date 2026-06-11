using Microsoft.Extensions.Logging;

namespace EncDotNet.S57.ExchangeSets;

/// <summary>
/// Verifies the integrity of the files referenced by an S-57 exchange set catalogue.
/// </summary>
/// <remarks>
/// <para>
/// Verification covers the per-file CRC checksums declared in the CATD <c>CRCS</c> subfield of
/// <c>CATALOG.031</c> (S-57 Edition 3.1 Part 3, clause 3.4) and, in future, S-63 digital
/// signatures. The two dimensions are reported independently on each
/// <see cref="S57FileVerificationResult"/>.
/// </para>
/// <para>
/// Implementations are non-throwing: per-file failures are surfaced as
/// <see cref="S57VerificationOutcome"/> values rather than exceptions.
/// </para>
/// </remarks>
public interface IS57ExchangeSetVerifier
{
    /// <summary>
    /// Verifies the integrity metadata in <paramref name="catalog"/> against the files located
    /// under <paramref name="rootPath"/>.
    /// </summary>
    /// <param name="rootPath">The absolute path to the root directory of the exchange set.</param>
    /// <param name="catalog">The parsed <c>CATALOG.031</c> whose entries to verify.</param>
    /// <param name="trustAnchors">
    /// Optional S-63 trust anchor options. Currently unused because signature verification is a
    /// seam; supply when S-63 support is available.
    /// </param>
    /// <param name="logger">An optional logger for reporting verification warnings.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result enumerating per-file verification outcomes without throwing.</returns>
    Task<S57ExchangeSetVerificationResult> VerifyAsync(
        string rootPath,
        S57Catalog catalog,
        S63TrustAnchorOptions? trustAnchors = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default);
}
