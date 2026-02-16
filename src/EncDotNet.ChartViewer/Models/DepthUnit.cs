namespace EncDotNet.ChartViewer.Models;

/// <summary>
/// Units of measurement for displaying sounding depths.
/// </summary>
public enum DepthUnit
{
    /// <summary>Display depths in feet (truncated to whole numbers).</summary>
    Feet,

    /// <summary>Display depths in meters (1/10 precision).</summary>
    Meters,

    /// <summary>Display depths in fathoms (1/10 precision).</summary>
    Fathoms,
}
