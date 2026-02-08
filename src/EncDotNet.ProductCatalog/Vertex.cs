using System.Xml.Serialization;

namespace EncDotNet.ProductCatalog;

public class Vertex
{
    [XmlElement("lat")]
    public double Latitude { get; set; }

    [XmlElement("long")]
    public double Longitude { get; set; }
}
