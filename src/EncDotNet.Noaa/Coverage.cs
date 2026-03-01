using System.Xml.Serialization;

namespace EncDotNet.Noaa;

/// <summary>
/// Represents the geographic coverage of an ENC cell, defined by one or more panels.
/// </summary>
public class Coverage
{
    /// <summary>Gets or sets the list of panels that define the coverage area.</summary>
    [XmlElement("panel")]
    public List<Panel> Panels { get; set; } = new();
}
