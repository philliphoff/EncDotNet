using System.Collections.Immutable;

namespace EncDotNet.S57.ExchangeSets;

/// <summary>
/// The aggregate result of verifying all files referenced by an S-57 exchange set catalogue.
/// </summary>
public sealed class S57ExchangeSetVerificationResult
{
    /// <summary>Gets the per-file verification results.</summary>
    public required ImmutableArray<S57FileVerificationResult> FileResults { get; init; }

    /// <summary>
    /// Gets a value indicating whether the exchange set has no integrity violations: every
    /// file's checksum is <see cref="S57VerificationOutcome.Ok"/> or
    /// <see cref="S57VerificationOutcome.NoChecksum"/> (the CRC is optional in S-57, so its
    /// absence is not a failure), and every signature is <see cref="S57VerificationOutcome.Ok"/>
    /// or <see cref="S57VerificationOutcome.NotSigned"/>.
    /// </summary>
    public bool AllValid => FileResults.All(r =>
        r.ChecksumOutcome is S57VerificationOutcome.Ok or S57VerificationOutcome.NoChecksum
        && r.SignatureOutcome is S57VerificationOutcome.Ok or S57VerificationOutcome.NotSigned);

    /// <summary>Gets a value indicating whether at least one file's CRC checksum did not match.</summary>
    public bool HasChecksumMismatches => FileResults.Any(r => r.ChecksumOutcome == S57VerificationOutcome.ChecksumMismatch);

    /// <summary>Gets a value indicating whether at least one referenced file was missing on disk.</summary>
    public bool HasMissingFiles => FileResults.Any(r => r.ChecksumOutcome == S57VerificationOutcome.FileMissing);

    /// <summary>
    /// Gets a value indicating whether at least one file had an invalid digital signature.
    /// </summary>
    public bool HasInvalidSignatures => FileResults.Any(r => r.SignatureOutcome == S57VerificationOutcome.SignatureInvalid);

    /// <summary>
    /// Gets a value indicating whether no file carries a digital signature (all are
    /// <see cref="S57VerificationOutcome.NotSigned"/>). Unencrypted ENC exchange sets are unsigned.
    /// </summary>
    public bool IsUnsigned => FileResults.All(r => r.SignatureOutcome == S57VerificationOutcome.NotSigned);
}
