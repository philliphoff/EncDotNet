using System.Xml.Serialization;

namespace EncDotNet.Noaa;

/// <summary>
/// Represents an individual ENC cell (chart) entry in the NOAA product catalog.
/// </summary>
public class Cell
{
    /// <summary>Gets or sets the cell name (e.g., "US5VA51M").</summary>
    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the long descriptive name of the cell.</summary>
    [XmlElement("lname")]
    public string LongName { get; set; } = string.Empty;

    /// <summary>Gets or sets the compilation scale (e.g., 22000 for 1:22,000).</summary>
    [XmlElement("cscale")]
    public int ChartScale { get; set; }

    /// <summary>Gets or sets the cell status.</summary>
    [XmlElement("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the U.S. Coast Guard districts associated with this cell.</summary>
    [XmlElement("coast_guard_districts")]
    public CoastGuardDistricts CoastGuardDistricts { get; set; } = new();

    /// <summary>Gets or sets the U.S. states associated with this cell.</summary>
    [XmlElement("states")]
    public States States { get; set; } = new();

    /// <summary>Gets or sets the NOAA charting regions associated with this cell.</summary>
    [XmlElement("regions")]
    public Regions Regions { get; set; } = new();

    /// <summary>Gets or sets the URL of the ZIP file containing this cell's data.</summary>
    [XmlElement("zipfile_location")]
    public string ZipfileLocation { get; set; } = string.Empty;

    /// <summary>Gets or sets the ZIP file date/time as a string.</summary>
    [XmlElement("zipfile_datetime")]
    public string ZipfileDatetime { get; set; } = string.Empty;

    /// <summary>Gets or sets the ZIP file date/time in ISO 8601 format.</summary>
    [XmlElement("zipfile_datetime_iso8601")]
    public DateTime ZipfileDatetimeIso8601 { get; set; }

    /// <summary>Gets or sets the size of the ZIP file in bytes.</summary>
    [XmlElement("zipfile_size")]
    public long ZipfileSize { get; set; }

    /// <summary>Gets or sets the edition number of this cell.</summary>
    [XmlElement("edtn")]
    public int Edition { get; set; }

    /// <summary>Gets or sets the update number applied to this cell.</summary>
    [XmlElement("updn")]
    public int UpdateNumber { get; set; }

    /// <summary>Gets or sets the update application date.</summary>
    [XmlElement("uadt")]
    public string UpdateApplicationDate { get; set; } = string.Empty;

    /// <summary>Gets or sets the issue date of this cell.</summary>
    [XmlElement("isdt")]
    public string IssueDate { get; set; } = string.Empty;

    /// <summary>Gets or sets the geographic coverage information for this cell.</summary>
    [XmlElement("cov")]
    public Coverage? Coverage { get; set; }
}
