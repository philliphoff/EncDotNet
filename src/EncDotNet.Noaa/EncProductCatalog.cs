using System.Xml.Serialization;

namespace EncDotNet.Noaa;

/// <summary>
/// Represents a NOAA ENC product catalog, containing a header and a collection of ENC cells.
/// </summary>
[XmlRoot("EncProductCatalog")]
public class EncProductCatalog
{
    /// <summary>Gets or sets the catalog header metadata.</summary>
    [XmlElement("Header")]
    public CatalogHeader Header { get; set; } = new();

    /// <summary>Gets or sets the list of ENC cells in the catalog.</summary>
    [XmlElement("cell")]
    public List<Cell> Cells { get; set; } = new();
}
