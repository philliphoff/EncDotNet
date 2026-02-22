using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using EncDotNet.Enc;
using EncDotNet.Enc.Charts;
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
        var codeSet = objectCodes.ToHashSet();
        var features = new List<IFeature>();

        foreach (var areaFeature in chart.AreaFeatures)
        {
            if (!codeSet.Contains(areaFeature.ObjectCode))
                continue;

            var template = S57LayerTemplates.GetTemplate(areaFeature.ObjectCode);
            var handler = template.Area ?? S57LayerTemplates.Default.Area;
            if (handler != null)
                features.AddRange(handler(chart, areaFeature));
        }

        foreach (var lineFeature in chart.LineFeatures)
        {
            if (!codeSet.Contains(lineFeature.ObjectCode))
                continue;

            var template = S57LayerTemplates.GetTemplate(lineFeature.ObjectCode);
            var handler = template.Line ?? S57LayerTemplates.Default.Line;
            if (handler != null)
                features.AddRange(handler(chart, lineFeature));
        }

        foreach (var pointFeature in chart.PointFeatures)
        {
            if (!codeSet.Contains(pointFeature.ObjectCode))
                continue;

            var template = S57LayerTemplates.GetTemplate(pointFeature.ObjectCode);
            var handler = template.Point ?? S57LayerTemplates.Default.Point;
            if (handler != null)
                features.AddRange(handler(chart, pointFeature, depthUnit));
        }

        double maxVisible = double.MaxValue;
        foreach (var code in objectCodes)
        {
            var template = S57LayerTemplates.GetTemplate(code);
            maxVisible = Math.Min(maxVisible, template.MaxVisible);
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
