using System.Xml.Serialization;

namespace EncDotNet.Noaa;

// HTTP Client Wrapper
public class EncProductCatalogClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly XmlSerializer _serializer;
    private bool _disposed;

    public EncProductCatalogClient(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _serializer = new XmlSerializer(typeof(EncProductCatalog));
    }

    public async Task<EncProductCatalog> GetCatalogAsync(string url, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var catalog = _serializer.Deserialize(stream) as EncProductCatalog;

        return catalog ?? throw new InvalidOperationException("Failed to deserialize catalog");
    }

    public async Task<EncProductCatalog> GetNoaaCatalogAsync(CancellationToken cancellationToken = default)
    {
        const string noaaUrl = "https://www.charts.noaa.gov/ENCs/ENCProdCat.xml";
        return await GetCatalogAsync(noaaUrl, cancellationToken);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}
