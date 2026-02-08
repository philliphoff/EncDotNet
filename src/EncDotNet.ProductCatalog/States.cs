using System.Xml.Serialization;

namespace EncDotNet.ProductCatalog;

public class States
{
    [XmlElement("state")]
    public List<string> StateList { get; set; } = new();
}
