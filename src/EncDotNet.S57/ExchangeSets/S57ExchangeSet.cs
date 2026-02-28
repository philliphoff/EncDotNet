using System.Collections.Immutable;

using EncDotNet.S57.Charts;
using Microsoft.Extensions.Logging;

namespace EncDotNet.S57.ExchangeSets;

/// <summary>
/// Represents the file layout of an S-57 ENC exchange set.
/// </summary>
/// <remarks>
/// <para>
/// An exchange set is a distribution package for S-57 chart data. It contains a
/// <c>CATALOG.031</c> catalogue file, a base cell file (with a <c>.000</c> extension),
/// and zero or more update files (with sequential extensions <c>.001</c>, <c>.002</c>, etc.).
/// </para>
/// <para>
/// All paths exposed by this type are relative to the root directory of the exchange set.
/// This type describes only the file layout; it does not parse the files themselves.
/// </para>
/// </remarks>
public sealed class S57ExchangeSet
{
    /// <summary>Gets the relative path to the catalogue file (e.g. <c>CATALOG.031</c>).</summary>
    public required string CatalogFileName { get; init; }

    /// <summary>Gets the relative path to the base cell file (e.g. <c>US5CA12M/US5CA12M.000</c>).</summary>
    public required string BaseCellFileName { get; init; }

    /// <summary>
    /// Gets the relative paths to the update files, ordered by application sequence
    /// (e.g. <c>US5CA12M/US5CA12M.001</c>, <c>US5CA12M/US5CA12M.002</c>).
    /// </summary>
    public required ImmutableArray<string> UpdateFileNames { get; init; }

    /// <summary>
    /// Reads the <c>CATALOG.031</c> file from the exchange set.
    /// </summary>
    /// <param name="rootPath">The absolute path to the root directory of the exchange set.</param>
    /// <param name="logger">An optional logger for reporting parsing warnings.</param>
    /// <returns>The parsed catalog.</returns>
    public S57Catalog ReadCatalog(string rootPath, ILogger? logger = null)
    {
        string catalogPath = Path.Combine(rootPath, CatalogFileName);
        return S57CatalogReader.ReadFromFile(catalogPath, logger);
    }

    /// <summary>
    /// Asynchronously reads the <c>CATALOG.031</c> file from the exchange set.
    /// </summary>
    /// <param name="rootPath">The absolute path to the root directory of the exchange set.</param>
    /// <param name="logger">An optional logger for reporting parsing warnings.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public Task<S57Catalog> ReadCatalogAsync(string rootPath, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        string catalogPath = Path.Combine(rootPath, CatalogFileName);
        return S57CatalogReader.ReadFromFileAsync(catalogPath, logger, cancellationToken);
    }

    /// <summary>
    /// Reads the base cell file and applies any update files in order, returning the resulting document.
    /// </summary>
    /// <param name="rootPath">The absolute path to the root directory of the exchange set.</param>
    /// <param name="logger">An optional logger for reporting parsing warnings.</param>
    /// <returns>The fully updated S-57 document.</returns>
    public S57Document ReadDocument(string rootPath, ILogger? logger = null)
    {
        string baseCellPath = Path.Combine(rootPath, BaseCellFileName);
        var document = S57DocumentReader.ReadFromFile(baseCellPath, logger);

        foreach (string updateFileName in UpdateFileNames)
        {
            string updatePath = Path.Combine(rootPath, updateFileName);
            var update = S57DocumentReader.ReadFromFile(updatePath, logger);
            document = document.ApplyChanges(update);
        }

        return document;
    }

    /// <summary>
    /// Asynchronously reads the base cell file and applies any update files in order, returning the resulting document.
    /// </summary>
    /// <param name="rootPath">The absolute path to the root directory of the exchange set.</param>
    /// <param name="logger">An optional logger for reporting parsing warnings.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public async Task<S57Document> ReadDocumentAsync(string rootPath, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        string baseCellPath = Path.Combine(rootPath, BaseCellFileName);
        var document = await S57DocumentReader.ReadFromFileAsync(baseCellPath, logger, cancellationToken).ConfigureAwait(false);

        foreach (string updateFileName in UpdateFileNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string updatePath = Path.Combine(rootPath, updateFileName);
            var update = await S57DocumentReader.ReadFromFileAsync(updatePath, logger, cancellationToken).ConfigureAwait(false);
            document = document.ApplyChanges(update);
        }

        return document;
    }

    /// <summary>
    /// Reads the base cell file and applies any update files in order, returning the resulting chart.
    /// </summary>
    /// <param name="rootPath">The absolute path to the root directory of the exchange set.</param>
    /// <param name="logger">An optional logger for reporting parsing warnings.</param>
    /// <returns>A strongly-typed chart model with all updates applied.</returns>
    public S57Chart ReadChart(string rootPath, ILogger? logger = null)
    {
        return S57Chart.FromDocument(ReadDocument(rootPath, logger));
    }

    /// <summary>
    /// Asynchronously reads the base cell file and applies any update files in order, returning the resulting chart.
    /// </summary>
    /// <param name="rootPath">The absolute path to the root directory of the exchange set.</param>
    /// <param name="logger">An optional logger for reporting parsing warnings.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    public async Task<S57Chart> ReadChartAsync(string rootPath, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var document = await ReadDocumentAsync(rootPath, logger, cancellationToken).ConfigureAwait(false);
        return S57Chart.FromDocument(document);
    }
}
