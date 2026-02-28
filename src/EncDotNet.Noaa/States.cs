using System.Xml.Serialization;

namespace EncDotNet.Noaa;

public class States
{
    [XmlElement("state")]
    public List<string> StateList { get; set; } = new();
}
