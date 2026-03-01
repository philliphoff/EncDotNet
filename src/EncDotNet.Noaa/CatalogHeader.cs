using System.Xml.Serialization;

namespace EncDotNet.Noaa;

/// <summary>
/// Represents the header section of a NOAA ENC product catalog, containing metadata about the catalog itself.
/// </summary>
public class CatalogHeader
{
    /// <summary>Gets or sets the catalog title.</summary>
    [XmlElement("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the date the catalog was created.</summary>
    [XmlElement("date_created")]
    public string DateCreated { get; set; } = string.Empty;

    /// <summary>Gets or sets the time the catalog was created.</summary>
    [XmlElement("time_created")]
    public string TimeCreated { get; set; } = string.Empty;

    /// <summary>Gets or sets the date from which the catalog is valid.</summary>
    [XmlElement("date_valid")]
    public string DateValid { get; set; } = string.Empty;

    /// <summary>Gets or sets the time from which the catalog is valid.</summary>
    [XmlElement("time_valid")]
    public string TimeValid { get; set; } = string.Empty;

    /// <summary>Gets or sets the validity date and time as a <see cref="DateTime"/>.</summary>
    [XmlElement("dt_valid")]
    public DateTime DtValid { get; set; }

    /// <summary>Gets or sets the reference specification (e.g., "ENC").</summary>
    [XmlElement("ref_spec")]
    public string RefSpec { get; set; } = string.Empty;

    /// <summary>Gets or sets the reference specification version.</summary>
    [XmlElement("ref_spec_vers")]
    public string RefSpecVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the S-62 agency code identifying the producing agency.</summary>
    [XmlElement("s62AgencyCode")]
    public int S62AgencyCode { get; set; }
}
