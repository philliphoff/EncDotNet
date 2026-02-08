using System.Xml.Serialization;

namespace EncDotNet.ProductCatalog;

public class Coverage
{
    [XmlElement("panel")]
    public List<Panel> Panels { get; set; } = new();
}
