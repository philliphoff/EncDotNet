using System.Xml.Serialization;

namespace EncDotNet.Noaa;

/// <summary>
/// HTTP client for downloading and deserializing NOAA ENC product catalogs.
/// </summary>
public class EncProductCatalogClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly XmlSerializer _serializer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="EncProductCatalogClient"/> class.
    /// </summary>
    /// <param name="httpClient">An optional <see cref="HttpClient"/> to use for requests. If <see langword="null"/>, a new instance is created.</param>
    public EncProductCatalogClient(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _serializer = new XmlSerializer(typeof(EncProductCatalog));
    }

    /// <summary>
    /// Downloads and deserializes an ENC product catalog from the specified URL.
    /// </summary>
    /// <param name="url">The URL of the catalog XML.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized <see cref="EncProductCatalog"/>.</returns>
    public async Task<EncProductCatalog> GetCatalogAsync(string url, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var catalog = _serializer.Deserialize(stream) as EncProductCatalog;

        return catalog ?? throw new InvalidOperationException("Failed to deserialize catalog");
    }

    /// <summary>
    /// Downloads and deserializes the official NOAA ENC product catalog.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The deserialized <see cref="EncProductCatalog"/>.</returns>
    public async Task<EncProductCatalog> GetNoaaCatalogAsync(CancellationToken cancellationToken = default)
    {
        const string noaaUrl = "https://www.charts.noaa.gov/ENCs/ENCProdCat.xml";
        return await GetCatalogAsync(noaaUrl, cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}
