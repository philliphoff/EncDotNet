using System.Xml.Serialization;

namespace EncDotNet.Noaa;

/// <summary>
/// Represents the NOAA charting regions associated with an ENC cell.
/// </summary>
public class Regions
{
    /// <summary>Gets or sets the list of region numbers.</summary>
    [XmlElement("region")]
    public List<int> RegionList { get; set; } = new();
}
