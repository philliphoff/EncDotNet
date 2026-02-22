using System;
using System.Collections.Generic;
using EncDotNet.Enc.Charts;
using EncDotNet.ChartViewer.Models;
using Mapsui;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using NetTopologySuite.Geometries;

namespace EncDotNet.ChartViewer;

/// <summary>
/// Defines how features of a specific S-57 object code are rendered as Mapsui features.
/// Each handler is optional; when absent, the default template's handler is used.
/// </summary>
public sealed class S57LayerTemplate
{
    /// <summary>
    /// Handler that produces Mapsui features from an S-57 area feature.
    /// Return an empty sequence to skip the feature.
    /// </summary>
    public Func<S57Chart, S57AreaFeature, IEnumerable<IFeature>>? Area { get; init; }

    /// <summary>
    /// Handler that produces Mapsui features from an S-57 line feature.
    /// Return an empty sequence to skip the feature.
    /// </summary>
    public Func<S57Chart, S57LineFeature, IEnumerable<IFeature>>? Line { get; init; }

    /// <summary>
    /// Handler that produces Mapsui features from an S-57 point feature.
    /// Return an empty sequence to skip the feature.
    /// </summary>
    public Func<S57Chart, S57PointFeature, DepthUnit, IEnumerable<IFeature>>? Point { get; init; }

    /// <summary>
    /// The maximum viewport resolution at which features using this template are visible.
    /// Defaults to <see cref="double.MaxValue"/> (always visible).
    /// </summary>
    public double MaxVisible { get; init; } = double.MaxValue;

    // --- Area handler helpers ---

    /// <summary>Creates an area handler that applies a fixed style.</summary>
    public static Func<S57Chart, S57AreaFeature, IEnumerable<IFeature>> AreaStyle(IStyle style)
        => (chart, feature) => CreateAreaFeature(chart, feature, style);

    /// <summary>Creates an area handler with simple fill and outline colors.</summary>
    public static Func<S57Chart, S57AreaFeature, IEnumerable<IFeature>> AreaStyle(Color fill, Color outline)
        => AreaStyle(new VectorStyle { Fill = new Brush(fill), Outline = new Pen(outline, 1) });

    /// <summary>Creates an area handler that determines the style per-feature. Return null to skip the feature.</summary>
    public static Func<S57Chart, S57AreaFeature, IEnumerable<IFeature>> AreaStyle(Func<S57AreaFeature, IStyle?> styleFactory)
        => (chart, feature) =>
        {
            var style = styleFactory(feature);
            return style != null ? CreateAreaFeature(chart, feature, style) : [];
        };

    // --- Line handler helpers ---

    /// <summary>Creates a line handler that applies a fixed style.</summary>
    public static Func<S57Chart, S57LineFeature, IEnumerable<IFeature>> LineStyle(IStyle style)
        => (chart, feature) => CreateLineFeature(chart, feature, style);

    /// <summary>Creates a line handler with a simple color and width.</summary>
    public static Func<S57Chart, S57LineFeature, IEnumerable<IFeature>> LineStyle(Color color, int width)
        => LineStyle(new VectorStyle { Line = new Pen(color, width) });

    // --- Point handler helpers ---

    /// <summary>Creates a point handler that applies a fixed style.</summary>
    public static Func<S57Chart, S57PointFeature, DepthUnit, IEnumerable<IFeature>> PointStyle(IStyle style)
        => (chart, feature, _) => CreatePointFeature(chart, feature, style);

    /// <summary>Creates a point handler that renders an image at the given scale.</summary>
    public static Func<S57Chart, S57PointFeature, DepthUnit, IEnumerable<IFeature>> ImagePointStyle(string source, double symbolScale)
        => PointStyle(new ImageStyle { Image = new Image { Source = source }, SymbolScale = symbolScale });

    // --- Geometry helpers (internal, shared with templates) ---

    internal static IEnumerable<IFeature> CreateAreaFeature(S57Chart chart, S57AreaFeature areaFeature, IStyle style)
    {
        var polygon = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, areaFeature);
        if (polygon != null)
        {
            var feature = new GeometryFeature(polygon);
            feature["ObjectCode"] = areaFeature.ObjectCode;
            feature.Styles.Add(style);
            yield return feature;
        }
    }

    internal static IEnumerable<IFeature> CreateLineFeature(S57Chart chart, S57LineFeature lineFeature, IStyle style)
    {
        var lineString = S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, lineFeature);
        if (lineString != null)
        {
            var feature = new GeometryFeature(lineString);
            feature["ObjectCode"] = lineFeature.ObjectCode;
            feature.Styles.Add(style);
            yield return feature;
        }
    }

    internal static IEnumerable<IFeature> CreatePointFeature(S57Chart chart, S57PointFeature pointFeature, IStyle style)
    {
        var point = CreatePointGeometry(chart, pointFeature);
        if (point != null)
        {
            var feature = new GeometryFeature(point);
            feature["ObjectCode"] = pointFeature.ObjectCode;
            feature.Styles.Add(style);
            yield return feature;
        }
    }

    internal static Point? CreatePointGeometry(S57Chart chart, S57PointFeature pointFeature)
    {
        if (!pointFeature.HasSpatialReferences)
            return null;

        var spatialRef = pointFeature.PrimarySpatialReference!.Value;

        var isolatedNode = chart.GetIsolatedNode(spatialRef.Name);
        if (isolatedNode?.HasPosition == true)
        {
            var (lon, lat) = chart.ToDecimalDegrees(isolatedNode.Position!.Value);
            var (x, y) = SphericalMercator.FromLonLat(lon, lat);
            return new Point(x, y);
        }

        var connectedNode = chart.GetConnectedNode(spatialRef.Name);
        if (connectedNode != null)
        {
            var (lon, lat) = chart.ToDecimalDegrees(connectedNode.Position);
            var (x, y) = SphericalMercator.FromLonLat(lon, lat);
            return new Point(x, y);
        }

        return null;
    }
}
