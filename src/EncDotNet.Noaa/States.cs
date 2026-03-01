using System.Xml.Serialization;

namespace EncDotNet.Noaa;

/// <summary>
/// Represents the U.S. states associated with an ENC cell.
/// </summary>
public class States
{
    /// <summary>Gets or sets the list of state names.</summary>
    [XmlElement("state")]
    public List<string> StateList { get; set; } = new();
}
