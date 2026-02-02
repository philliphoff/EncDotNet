using System.Xml.Serialization;

namespace EncDotNet.ProductCatalog;

// Models
[XmlRoot("EncProductCatalog")]
public class EncProductCatalog
{
    [XmlElement("Header")]
    public CatalogHeader Header { get; set; } = new();

    [XmlElement("cell")]
    public List<Cell> Cells { get; set; } = new();
}

public class CatalogHeader
{
    [XmlElement("title")]
    public string Title { get; set; } = string.Empty;

    [XmlElement("date_created")]
    public string DateCreated { get; set; } = string.Empty;

    [XmlElement("time_created")]
    public string TimeCreated { get; set; } = string.Empty;

    [XmlElement("date_valid")]
    public string DateValid { get; set; } = string.Empty;

    [XmlElement("time_valid")]
    public string TimeValid { get; set; } = string.Empty;

    [XmlElement("dt_valid")]
    public DateTime DtValid { get; set; }

    [XmlElement("ref_spec")]
    public string RefSpec { get; set; } = string.Empty;

    [XmlElement("ref_spec_vers")]
    public string RefSpecVersion { get; set; } = string.Empty;

    [XmlElement("s62AgencyCode")]
    public int S62AgencyCode { get; set; }
}

public class Cell
{
    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("lname")]
    public string LongName { get; set; } = string.Empty;

    [XmlElement("cscale")]
    public int ChartScale { get; set; }

    [XmlElement("status")]
    public string Status { get; set; } = string.Empty;

    [XmlElement("coast_guard_districts")]
    public CoastGuardDistricts CoastGuardDistricts { get; set; } = new();

    [XmlElement("states")]
    public States States { get; set; } = new();

    [XmlElement("regions")]
    public Regions Regions { get; set; } = new();

    [XmlElement("zipfile_location")]
    public string ZipfileLocation { get; set; } = string.Empty;

    [XmlElement("zipfile_datetime")]
    public string ZipfileDatetime { get; set; } = string.Empty;

    [XmlElement("zipfile_datetime_iso8601")]
    public DateTime ZipfileDatetimeIso8601 { get; set; }

    [XmlElement("zipfile_size")]
    public long ZipfileSize { get; set; }

    [XmlElement("edtn")]
    public int Edition { get; set; }

    [XmlElement("updn")]
    public int UpdateNumber { get; set; }

    [XmlElement("uadt")]
    public string UpdateApplicationDate { get; set; } = string.Empty;

    [XmlElement("isdt")]
    public string IssueDate { get; set; } = string.Empty;

    [XmlElement("cov")]
    public Coverage? Coverage { get; set; }
}

public class CoastGuardDistricts
{
    [XmlElement("coast_guard_district")]
    public List<int> Districts { get; set; } = new();
}

public class States
{
    [XmlElement("state")]
    public List<string> StateList { get; set; } = new();
}

public class Regions
{
    [XmlElement("region")]
    public List<int> RegionList { get; set; } = new();
}

public class Coverage
{
    [XmlElement("panel")]
    public List<Panel> Panels { get; set; } = new();
}

public class Panel
{
    [XmlElement("panel_no")]
    public int PanelNumber { get; set; }

    [XmlElement("type")]
    public string Type { get; set; } = string.Empty;

    [XmlElement("vertex")]
    public List<Vertex> Vertices { get; set; } = new();
}

public class Vertex
{
    [XmlElement("lat")]
    public double Latitude { get; set; }

    [XmlElement("long")]
    public double Longitude { get; set; }
}

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