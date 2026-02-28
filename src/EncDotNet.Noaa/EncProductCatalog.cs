using System.Xml.Serialization;

namespace EncDotNet.Noaa;

[XmlRoot("EncProductCatalog")]
public class EncProductCatalog
{
    [XmlElement("Header")]
    public CatalogHeader Header { get; set; } = new();

    [XmlElement("cell")]
    public List<Cell> Cells { get; set; } = new();
}
