using Mapsui.Styles;

namespace EncDotNet.ChartViewer;

/// <summary>
/// Helpers for applying SCAMIN (scale minimum) visibility to Mapsui styles.
/// </summary>
internal static class ScaminStyleHelper
{
    /// <summary>Approximate pixel size in meters at standard screen DPI (~96).</summary>
    private const double PixelSizeMeters = 0.000264583;

    /// <summary>
    /// Returns a copy of the given style with <see cref="IStyle.MaxVisible"/> set
    /// so the feature is hidden when the viewport is zoomed out beyond the SCAMIN scale.
    /// For shared style instances the clone avoids mutating the original.
    /// </summary>
    public static IStyle CloneWithScamin(IStyle style, int scamin)
    {
        double maxVisible = scamin * PixelSizeMeters;

        return style switch
        {
            // LabelStyle extends VectorStyle — must be matched first.
            // Text is init-only so we cannot clone; set MaxVisible in place.
            // This is safe because label styles are typically created per-feature.
            LabelStyle ls => SetMaxVisible(ls, maxVisible),
            ImageStyle ims => new ImageStyle
            {
                Image = ims.Image,
                SymbolScale = ims.SymbolScale,
                SymbolRotation = ims.SymbolRotation,
                Enabled = ims.Enabled,
                Opacity = ims.Opacity,
                MaxVisible = maxVisible,
            },
            VectorStyle vs => new VectorStyle
            {
                Fill = vs.Fill,
                Line = vs.Line,
                Outline = vs.Outline,
                Enabled = vs.Enabled,
                Opacity = vs.Opacity,
                MaxVisible = maxVisible,
            },
            _ => style, // Unknown type — return as-is (best effort).
        };
    }

    private static LabelStyle SetMaxVisible(LabelStyle ls, double maxVisible)
    {
        ls.MaxVisible = maxVisible;
        return ls;
    }
}
