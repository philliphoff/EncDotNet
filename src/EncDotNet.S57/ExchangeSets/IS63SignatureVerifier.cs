namespace EncDotNet.S57.ExchangeSets;

/// <summary>
/// Verifies the S-63 digital signature of a file in an S-57 exchange set.
/// </summary>
/// <remarks>
/// <para>
/// This is a <strong>seam</strong>: the full S-63 ENC Data Protection Scheme (RSA/DSA signatures
/// over the SA-issued data server certificate chain) is not yet implemented and is intended to
/// land alongside S-63 decryption support. The interface is published now so that the
/// <see cref="IS57ExchangeSetVerifier"/> surface is stable in advance.
/// </para>
/// <para>
/// Implementations must be non-throwing and report failures via the returned
/// <see cref="S57VerificationOutcome"/>.
/// </para>
/// </remarks>
public interface IS63SignatureVerifier
{
    /// <summary>
    /// Verifies the detached S-63 signature associated with the file at <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath">The absolute path to the file whose signature is being verified.</param>
    /// <param name="trustAnchors">Trust anchor options controlling certificate chain validation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The signature verification outcome. Returns <see cref="S57VerificationOutcome.NotSigned"/>
    /// when no signature material is present for the file.
    /// </returns>
    Task<S57VerificationOutcome> VerifySignatureAsync(
        string filePath,
        S63TrustAnchorOptions trustAnchors,
        CancellationToken cancellationToken = default);
}
