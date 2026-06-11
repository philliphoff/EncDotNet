using System.Security.Cryptography.X509Certificates;

namespace EncDotNet.S57.ExchangeSets;

/// <summary>
/// Options that configure the trust anchors used during S-63 exchange set signature verification.
/// </summary>
/// <remarks>
/// <para>
/// Mirrors the S-100 <c>TrustAnchorOptions</c> so that callers can configure both schemes
/// uniformly. Under the S-63 ENC Data Protection Scheme the IHO Scheme Administrator (SA)
/// public key is the root of trust; for interoperability testing the IHO publishes test SA keys.
/// </para>
/// <para>
/// S-63 signature verification is not yet implemented. This type is provided as a seam so that
/// the public verification API is stable before S-63 support lands.
/// </para>
/// </remarks>
public sealed class S63TrustAnchorOptions
{
    /// <summary>
    /// Gets the trusted root certificates (IHO Scheme Administrator public keys). When empty,
    /// certificate chain validation is governed by <see cref="AllowUntrustedCertificates"/>.
    /// </summary>
    public IReadOnlyList<X509Certificate2> TrustedRoots { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether signature verification proceeds even when the signing
    /// certificate cannot be chained to a trusted root. Useful for development and inspection.
    /// </summary>
    public bool AllowUntrustedCertificates { get; init; }
}
