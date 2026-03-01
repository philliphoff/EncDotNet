using System.Xml.Serialization;

namespace EncDotNet.Noaa;

/// <summary>
/// Represents the U.S. Coast Guard districts associated with an ENC cell.
/// </summary>
public class CoastGuardDistricts
{
    /// <summary>Gets or sets the list of Coast Guard district numbers.</summary>
    [XmlElement("coast_guard_district")]
    public List<int> Districts { get; set; } = new();
}
