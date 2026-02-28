using System.Xml.Serialization;

namespace EncDotNet.Noaa;

public class Panel
{
    [XmlElement("panel_no")]
    public int PanelNumber { get; set; }

    [XmlElement("type")]
    public string Type { get; set; } = string.Empty;

    [XmlElement("vertex")]
    public List<Vertex> Vertices { get; set; } = new();
}
