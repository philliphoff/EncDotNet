using System.Xml.Serialization;

namespace EncDotNet.Noaa;

/// <summary>
/// Represents a geographic panel within an ENC cell's coverage area, defined by a polygon of vertices.
/// </summary>
public class Panel
{
    /// <summary>Gets or sets the panel number.</summary>
    [XmlElement("panel_no")]
    public int PanelNumber { get; set; }

    /// <summary>Gets or sets the panel type.</summary>
    [XmlElement("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the vertices defining the panel's boundary polygon.</summary>
    [XmlElement("vertex")]
    public List<Vertex> Vertices { get; set; } = new();
}
