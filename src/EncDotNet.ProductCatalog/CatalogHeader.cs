using System.Xml.Serialization;

namespace EncDotNet.ProductCatalog;

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
