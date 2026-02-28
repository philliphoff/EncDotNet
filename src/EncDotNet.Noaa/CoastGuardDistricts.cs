using System.Xml.Serialization;

namespace EncDotNet.Noaa;

public class CoastGuardDistricts
{
    [XmlElement("coast_guard_district")]
    public List<int> Districts { get; set; } = new();
}
