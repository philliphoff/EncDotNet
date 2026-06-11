namespace EncDotNet.S57.ExchangeSets;

/// <summary>
/// The verification result for a single file referenced by an S-57 exchange set catalogue.
/// </summary>
/// <remarks>
/// The checksum and digital-signature outcomes are tracked as independent dimensions: a file
/// may have a valid CRC checksum yet carry no signature (the common case for unencrypted ENCs),
/// or vice versa. Inspect <see cref="ChecksumOutcome"/> and <see cref="SignatureOutcome"/>
/// separately.
/// </remarks>
public sealed class S57FileVerificationResult
{
    /// <summary>Gets the name of the file as declared in the catalogue (CATD <c>FILE</c> subfield).</summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the outcome of CRC checksum verification for this file.
    /// </summary>
    /// <remarks>
    /// Expected values are <see cref="S57VerificationOutcome.Ok"/>,
    /// <see cref="S57VerificationOutcome.NoChecksum"/>,
    /// <see cref="S57VerificationOutcome.ChecksumMismatch"/>,
    /// <see cref="S57VerificationOutcome.FileMissing"/>, or
    /// <see cref="S57VerificationOutcome.Error"/>.
    /// </remarks>
    public required S57VerificationOutcome ChecksumOutcome { get; init; }

    /// <summary>
    /// Gets the outcome of digital-signature (S-63) verification for this file.
    /// </summary>
    /// <remarks>
    /// S-63 signature verification is not yet implemented; this currently reports
    /// <see cref="S57VerificationOutcome.NotSigned"/> for all files. The property exists as a
    /// seam to be populated when S-63 support lands.
    /// </remarks>
    public S57VerificationOutcome SignatureOutcome { get; init; } = S57VerificationOutcome.NotSigned;

    /// <summary>Gets the CRC checksum declared in the catalogue, if any.</summary>
    public string? ExpectedCrc { get; init; }

    /// <summary>Gets the CRC checksum computed from the file on disk, if it was computed.</summary>
    public string? ActualCrc { get; init; }

    /// <summary>Gets an optional detail message (e.g. an exception message on <see cref="S57VerificationOutcome.Error"/>).</summary>
    public string? Detail { get; init; }
}
