namespace EncDotNet.S57.ExchangeSets;

/// <summary>
/// The outcome of verifying a single integrity dimension (checksum or digital signature)
/// of a file referenced by an S-57 exchange set catalogue.
/// </summary>
/// <remarks>
/// <para>
/// The member names are intentionally aligned with the S-100 exchange set verifier
/// (<c>EncDotNet.S100.ExchangeSets.VerificationOutcome</c>) so that a future S-57→S-101
/// bridge can treat both schemes uniformly. The S-57 enumeration additionally defines the
/// checksum-specific members <see cref="NoChecksum"/> and <see cref="ChecksumMismatch"/>.
/// </para>
/// <para>
/// Checksum verification is governed by S-57 Edition 3.1 Part 3, clause 3.4 and the relevant
/// ENC product specification (Appendix B.1). Signature verification is governed by the S-63
/// ENC Data Protection Scheme and is currently exposed as a seam only.
/// </para>
/// </remarks>
public enum S57VerificationOutcome
{
    /// <summary>The dimension was verified successfully.</summary>
    Ok,

    /// <summary>The file carries no digital signature.</summary>
    NotSigned,

    /// <summary>The digital signature does not match the file content.</summary>
    SignatureInvalid,

    /// <summary>The signing certificate is not trusted by the configured trust anchors.</summary>
    CertificateUntrusted,

    /// <summary>The signing certificate has expired.</summary>
    CertificateExpired,

    /// <summary>The referenced file was not found on disk.</summary>
    FileMissing,

    /// <summary>The referenced certificate was not found.</summary>
    CertificateNotFound,

    /// <summary>An unexpected error occurred during verification.</summary>
    Error,

    /// <summary>The catalogue entry declares no CRC checksum to verify against.</summary>
    NoChecksum,

    /// <summary>The computed CRC checksum does not match the value declared in the catalogue.</summary>
    ChecksumMismatch,
}
