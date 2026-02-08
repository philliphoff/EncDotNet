using System.Xml.Serialization;

namespace EncDotNet.ProductCatalog;

public class Regions
{
    [XmlElement("region")]
    public List<int> RegionList { get; set; } = new();
}
