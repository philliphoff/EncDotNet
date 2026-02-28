using System.Xml.Serialization;

namespace EncDotNet.Noaa;

public class Coverage
{
    [XmlElement("panel")]
    public List<Panel> Panels { get; set; } = new();
}
