using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EncDotNet.S57;
using EncDotNet.S57.Charts;
using EncDotNet.ChartViewer.Models;
using Mapsui;
using Mapsui.Layers;

namespace EncDotNet.ChartViewer;

/// <summary>
/// Factory for creating Mapsui layers from S-57 chart data.
/// </summary>
public static class S57LayerFactory
{
    /// <summary>
    /// Creates a MemoryLayer containing only features matching the specified object codes.
    /// </summary>
    /// <param name="chart">The S-57 chart to render.</param>
    /// <param name="objectCodes">The S-57 object codes to include.</param>
    /// <param name="layerName">Name for the layer.</param>
    /// <param name="depthUnit">The unit for displaying depth values.</param>
    /// <returns>A MemoryLayer containing the matching features.</returns>
    public static MemoryLayer CreateLayerForObjectCodes(
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

        return new MemoryLayer
        {
            Name = layerName,
            Features = features,
            Style = null,
            MaxVisible = maxVisible,
        };
    }
}
