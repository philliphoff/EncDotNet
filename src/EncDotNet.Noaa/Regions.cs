using System.Xml.Serialization;

namespace EncDotNet.Noaa;

public class Regions
{
    [XmlElement("region")]
    public List<int> RegionList { get; set; } = new();
}
