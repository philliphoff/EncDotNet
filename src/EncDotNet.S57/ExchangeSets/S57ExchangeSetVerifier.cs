using System.Collections.Immutable;
using Microsoft.Extensions.Logging;

namespace EncDotNet.S57.ExchangeSets;

/// <summary>
/// Default implementation of <see cref="IS57ExchangeSetVerifier"/>.
/// </summary>
/// <remarks>
/// <para>
/// Validates each catalogue entry's CRC checksum (CATD <c>CRCS</c> subfield) against the
/// corresponding file on disk using the CRC-32 algorithm described by <see cref="S57Crc32"/>.
/// </para>
/// <para>
/// S-63 digital-signature verification is delegated to an optional
/// <see cref="IS63SignatureVerifier"/>. When none is supplied (the default), every file is
/// reported as <see cref="S57VerificationOutcome.NotSigned"/>, which matches unencrypted ENC
/// exchange sets.
/// </para>
/// </remarks>
public sealed class S57ExchangeSetVerifier : IS57ExchangeSetVerifier
{
    private readonly IS63SignatureVerifier? _signatureVerifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="S57ExchangeSetVerifier"/> class.
    /// </summary>
    /// <param name="signatureVerifier">
    /// An optional S-63 signature verifier. When <see langword="null"/>, signature verification
    /// is skipped and all files report <see cref="S57VerificationOutcome.NotSigned"/>.
    /// </param>
    public S57ExchangeSetVerifier(IS63SignatureVerifier? signatureVerifier = null)
    {
        _signatureVerifier = signatureVerifier;
    }

    /// <inheritdoc />
    public async Task<S57ExchangeSetVerificationResult> VerifyAsync(
        string rootPath,
        S57Catalog catalog,
        S63TrustAnchorOptions? trustAnchors = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(rootPath);
        ArgumentNullException.ThrowIfNull(catalog);

        var anchors = trustAnchors ?? new S63TrustAnchorOptions();
        var results = ImmutableArray.CreateBuilder<S57FileVerificationResult>(catalog.Entries.Count);

        foreach (var entry in catalog.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(await VerifyEntryAsync(rootPath, entry, anchors, logger, cancellationToken).ConfigureAwait(false));
        }

        return new S57ExchangeSetVerificationResult { FileResults = results.ToImmutable() };
    }

    private async Task<S57FileVerificationResult> VerifyEntryAsync(
        string rootPath,
        S57CatalogEntry entry,
        S63TrustAnchorOptions trustAnchors,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        string displayName = string.IsNullOrEmpty(entry.FileName) ? entry.LongFileName : entry.FileName;

        // An entry without a declared CRC carries nothing to verify (e.g. the CATALOG.031
        // self-reference). Report NoChecksum without requiring the file to be present.
        if (string.IsNullOrWhiteSpace(entry.CrcChecksum))
        {
            return new S57FileVerificationResult
            {
                FileName = displayName,
                ChecksumOutcome = S57VerificationOutcome.NoChecksum,
                SignatureOutcome = S57VerificationOutcome.NotSigned,
            };
        }

        string? filePath = ResolveFilePath(rootPath, entry);
        if (filePath is null)
        {
            logger?.LogWarning("Exchange set file '{FileName}' referenced by the catalog was not found under '{RootPath}'.", displayName, rootPath);
            return new S57FileVerificationResult
            {
                FileName = displayName,
                ChecksumOutcome = S57VerificationOutcome.FileMissing,
                SignatureOutcome = S57VerificationOutcome.NotSigned,
                ExpectedCrc = entry.CrcChecksum,
            };
        }

        S57VerificationOutcome checksumOutcome;
        string? actualCrc = null;
        string? detail = null;

        try
        {
            await using var stream = new FileStream(
                filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 81920, useAsync: true);

            uint crc = await S57Crc32.ComputeAsync(stream, cancellationToken).ConfigureAwait(false);
            actualCrc = S57Crc32.Format(crc);

            checksumOutcome = string.Equals(actualCrc, NormalizeCrc(entry.CrcChecksum), StringComparison.OrdinalIgnoreCase)
                ? S57VerificationOutcome.Ok
                : S57VerificationOutcome.ChecksumMismatch;

            if (checksumOutcome == S57VerificationOutcome.ChecksumMismatch)
            {
                logger?.LogWarning(
                    "CRC mismatch for '{FileName}': expected {Expected}, computed {Actual}.",
                    displayName, entry.CrcChecksum, actualCrc);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            checksumOutcome = S57VerificationOutcome.Error;
            detail = ex.Message;
            logger?.LogWarning(ex, "Failed to verify CRC for '{FileName}'.", displayName);
        }

        var signatureOutcome = _signatureVerifier is null
            ? S57VerificationOutcome.NotSigned
            : await _signatureVerifier.VerifySignatureAsync(filePath, trustAnchors, cancellationToken).ConfigureAwait(false);

        return new S57FileVerificationResult
        {
            FileName = displayName,
            ChecksumOutcome = checksumOutcome,
            SignatureOutcome = signatureOutcome,
            ExpectedCrc = entry.CrcChecksum,
            ActualCrc = actualCrc,
            Detail = detail,
        };
    }

    /// <summary>
    /// Resolves a catalogue entry to an existing file under the exchange set root, trying the
    /// short <see cref="S57CatalogEntry.FileName"/> first and then the
    /// <see cref="S57CatalogEntry.LongFileName"/>. Returns <see langword="null"/> if neither
    /// candidate exists.
    /// </summary>
    private static string? ResolveFilePath(string rootPath, S57CatalogEntry entry)
    {
        foreach (string? candidate in new[] { entry.FileName, entry.LongFileName })
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            string relative = candidate.Replace('\\', '/');
            string fullPath = Path.Combine(rootPath, Path.Combine(relative.Split('/')));
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    /// <summary>
    /// Normalizes a CRC string from the catalogue for comparison: trims whitespace and pads to
    /// the canonical 8 hexadecimal digits (the value is stored most-significant byte first).
    /// </summary>
    private static string NormalizeCrc(string crc)
    {
        string trimmed = crc.Trim();
        return trimmed.Length < 8 ? trimmed.PadLeft(8, '0') : trimmed;
    }
}
