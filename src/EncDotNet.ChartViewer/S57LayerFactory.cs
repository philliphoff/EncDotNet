using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EncDotNet.S57;
using EncDotNet.S57.Charts;
using EncDotNet.ChartViewer.Models;
using EncDotNet.ChartViewer.ViewModels;
using Mapsui;
using Mapsui.Layers;

namespace EncDotNet.ChartViewer;

/// <summary>
/// Factory for creating Mapsui layers from S-57 chart data.
/// </summary>
public static class S57LayerFactory
{
    /// <summary>Approximate pixel size in meters at standard screen DPI (~96).</summary>
    internal const double PixelSizeMeters = 0.000264583;

    /// <summary>
    /// Allow displaying chart features at up to 2x beyond the chart's compilation scale
    /// before hiding. This avoids abruptly hiding a chart right at its nominal scale and
    /// matches common ECDIS over-scale tolerance practice.
    /// </summary>
    internal const double OverScaleFactor = 2.0;

    /// <summary>
    /// Creates a MemoryLayer containing only features matching the specified object codes.
    /// Returns <c>null</c> when the chart contains no renderable features for the given codes,
    /// allowing callers to skip adding empty layers and reduce Mapsui overhead.
    /// </summary>
    /// <param name="chart">The S-57 chart to render.</param>
    /// <param name="objectCodes">The S-57 object codes to include.</param>
    /// <param name="layerName">Name for the layer.</param>
    /// <param name="depthUnit">The unit for displaying depth values.</param>
    /// <returns>A MemoryLayer containing the matching features, or <c>null</c> if no features were produced.</returns>
    public static MemoryLayer? CreateLayerForObjectCodes(
        S57Chart chart,
        ImmutableArray<S57ObjectCode> objectCodes,
        string layerName,
        DepthUnit depthUnit = DepthUnit.Feet)
    {
        var features = new List<IFeature>();
        double maxVisible = double.MaxValue;

        foreach (var code in objectCodes)
        {
            var template = S57LayerTemplates.GetTemplate(code);
            maxVisible = Math.Min(maxVisible, template.MaxVisible);

            if (!chart.FeaturesByObjectCode.TryGetValue(code, out var codeFeatures))
                continue;

            var areaHandler = template.Area ?? S57LayerTemplates.Default.Area;
            if (areaHandler != null)
            {
                foreach (var areaFeature in codeFeatures.Areas)
                    features.AddRange(areaHandler(chart, areaFeature));
            }

            var lineHandler = template.Line ?? S57LayerTemplates.Default.Line;
            if (lineHandler != null)
            {
                foreach (var lineFeature in codeFeatures.Lines)
                    features.AddRange(lineHandler(chart, lineFeature));
            }

            var pointHandler = template.Point ?? S57LayerTemplates.Default.Point;
            if (pointHandler != null)
            {
                foreach (var pointFeature in codeFeatures.Points)
                    features.AddRange(pointHandler(chart, pointFeature, depthUnit));
            }
        }

        if (features.Count == 0)
            return null;

        // Sort features by FeatureOrder if present, so that e.g. deeper DEPARE areas
        // are drawn first and shallower areas render on top.
        features.Sort((a, b) =>
        {
            var orderA = a["FeatureOrder"] is double da ? da : 0.0;
            var orderB = b["FeatureOrder"] is double db ? db : 0.0;
            return orderA.CompareTo(orderB);
        });

        // Limit layer visibility based on the chart's compilation scale (CSCL).
        // A 1:22,000 chart should not render when the viewport is zoomed out to 1:500,000.
        double cscl = chart.CompilationScale;
        double csclMaxVisible = cscl > 0
            ? cscl * PixelSizeMeters * OverScaleFactor
            : double.MaxValue;

        return new MemoryLayer
        {
            Name = layerName,
            Features = features,
            Style = null,
            MaxVisible = Math.Min(maxVisible, csclMaxVisible),
        };
    }

    /// <summary>
    /// Recalculates <see cref="MemoryLayer.MinVisible"/> for all loaded charts so that
    /// coarse overview charts are suppressed when sufficiently detailed charts are available,
    /// while ensuring there are no zoom-level gaps where nothing renders.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The algorithm sorts loaded charts by compilation scale (finest first) and sets each
    /// chart's MinVisible to the MaxVisible of the chart <b>two</b> scale levels finer.
    /// This guarantees at least two adjacent scale levels are visible at every resolution,
    /// providing geographic fallback coverage (since finer charts may not geographically
    /// cover the same areas as coarser ones) while still suppressing very coarse overview
    /// charts that would cause visual bleed-through.
    /// </para>
    /// <para>
    /// The three finest scale levels keep MinVisible = 0 so they are always visible when
    /// zoomed in.
    /// </para>
    /// </remarks>
    internal static void RecalculateMinVisible(IReadOnlyCollection<ChartViewModel> loadedCharts)
    {
        if (loadedCharts.Count == 0)
            return;

        // Build a list of (CSCL, MaxVisible) for distinct scales, sorted ascending (finest first).
        // Multiple charts at the same scale share the same visibility band.
        var scaleMaxVisible = new SortedDictionary<int, double>();
        foreach (var chartVm in loadedCharts)
        {
            int cscl = chartVm.CompilationScale;
            if (cscl <= 0)
                continue;

            double maxVis = cscl * PixelSizeMeters * OverScaleFactor;

            // A chart's layers may have template-specific MaxVisible lower than the CSCL-based
            // limit. Use the CSCL-based value for the cascade since it represents the chart's
            // overall visibility ceiling.
            if (!scaleMaxVisible.ContainsKey(cscl))
                scaleMaxVisible[cscl] = maxVis;
        }

        // Build a mapping from CSCL → MinVisible.
        // Iterate scales from finest (lowest CSCL) to coarsest (highest CSCL).
        // Each chart's MinVisible is the MaxVisible of the chart three scale levels finer,
        // ensuring at least three scale levels overlap at every resolution. This provides
        // geographic fallback coverage (since finer charts rarely cover the same full area
        // as coarser ones) while still suppressing vastly coarser overview charts whose
        // geometry would cause visual bleed-through.
        const int overlapLevels = 3;
        var scales = new List<KeyValuePair<int, double>>(scaleMaxVisible);
        var scaleMinVisible = new Dictionary<int, double>();
        for (int i = 0; i < scales.Count; i++)
        {
            // The finest N scales (i < overlapLevels) keep MinVisible = 0.
            // Coarser scales reference the chart N levels finer.
            double minVis = i >= overlapLevels ? scales[i - overlapLevels].Value : 0;
            scaleMinVisible[scales[i].Key] = minVis;
        }

        // Apply MinVisible to all layers of each loaded chart.
        foreach (var chartVm in loadedCharts)
        {
            int cscl = chartVm.CompilationScale;
            double minVis = cscl > 0 && scaleMinVisible.TryGetValue(cscl, out var mv) ? mv : 0;

            foreach (var layer in chartVm.Layers)
            {
                layer.MinVisible = minVis;
            }
        }
    }
}
