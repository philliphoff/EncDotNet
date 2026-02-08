using System.Xml.Serialization;

namespace EncDotNet.ProductCatalog;

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
