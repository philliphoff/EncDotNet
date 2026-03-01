using System.Xml.Serialization;

namespace EncDotNet.Noaa;

/// <summary>
/// Represents a geographic vertex (latitude/longitude point) used to define coverage panel boundaries.
/// </summary>
public class Vertex
{
    /// <summary>Gets or sets the latitude in decimal degrees.</summary>
    [XmlElement("lat")]
    public double Latitude { get; set; }

    /// <summary>Gets or sets the longitude in decimal degrees.</summary>
    [XmlElement("long")]
    public double Longitude { get; set; }
}
