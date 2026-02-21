using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using EncDotNet.Enc;
using EncDotNet.Enc.Charts;
using EncDotNet.ChartViewer.Models;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using NetTopologySuite.Geometries;

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
    /// <returns>A MemoryLayer containing the matching features.</returns>
    public static MemoryLayer CreateLayerForObjectCodes(
        S57Chart chart,
        ImmutableArray<S57ObjectCode> objectCodes,
        string layerName,
        DepthUnit depthUnit = DepthUnit.Feet)
    {
        var codeSet = objectCodes.ToHashSet();
        var features = new List<IFeature>();

        // Add matching area features
        foreach (var areaFeature in chart.AreaFeatures)
        {
            if (!codeSet.Contains(areaFeature.ObjectCode))
                continue;

            // For DEPARE, categorize by depth band and skip deep areas
            if (areaFeature.ObjectCode == S57ObjectCode.DEPARE)
            {
                var style = CreateDepthAreaStyle(areaFeature);
                if (style == null)
                    continue; // deep band: not displayed

                var polygon = CreatePolygonFromAreaFeature(chart, areaFeature);
                if (polygon != null)
                {
                    var feature = new GeometryFeature(polygon);
                    feature["ObjectCode"] = areaFeature.ObjectCode;
                    feature.Styles.Add(style);
                    features.Add(feature);
                }
                continue;
            }

            var polygon2 = CreatePolygonFromAreaFeature(chart, areaFeature);
            if (polygon2 != null)
            {
                var feature = new GeometryFeature(polygon2);
                feature["ObjectCode"] = areaFeature.ObjectCode;
                feature.Styles.Add(CreateAreaStyle(areaFeature.ObjectCode));
                features.Add(feature);
            }
        }

        // Add matching line features
        foreach (var lineFeature in chart.LineFeatures)
        {
            if (!codeSet.Contains(lineFeature.ObjectCode))
                continue;

            var lineString = CreateLineStringFromLineFeature(chart, lineFeature);
            if (lineString != null)
            {
                var feature = new GeometryFeature(lineString);
                feature["ObjectCode"] = lineFeature.ObjectCode;
                feature.Styles.Add(CreateLineStyle(lineFeature.ObjectCode));
                features.Add(feature);
            }
        }

        // Add matching point features
        foreach (var pointFeature in chart.PointFeatures)
        {
            if (!codeSet.Contains(pointFeature.ObjectCode))
                continue;

            if (pointFeature.ObjectCode == S57ObjectCode.SOUNDG)
            {
                features.AddRange(CreateSoundingFeatures(chart, pointFeature, depthUnit));
                continue;
            }

            var point = CreatePointFromPointFeature(chart, pointFeature);
            if (point != null)
            {
                var feature = new GeometryFeature(point);
                feature["ObjectCode"] = pointFeature.ObjectCode;
                feature.Styles.Add(CreatePointStyle(pointFeature.ObjectCode));
                features.Add(feature);
            }
        }

        return new MemoryLayer
        {
            Name = layerName,
            Features = features,
            Style = null, // Use per-feature styles
            MaxVisible = codeSet.Contains(S57ObjectCode.SOUNDG) && codeSet.Count == 1 ? SoundingMaxResolution : double.MaxValue,
        };
    }

    /// <summary>
    /// The maximum viewport resolution at which soundings are visible.
    /// Roughly corresponds to OSM zoom level ~16; soundings are hidden when zoomed out further.
    /// </summary>
    private const double SoundingMaxResolution = 10;

    private static IEnumerable<IFeature> CreateSoundingFeatures(S57Chart chart, S57PointFeature pointFeature, DepthUnit depthUnit = DepthUnit.Feet)
    {
        if (!pointFeature.HasSpatialReferences)
            yield break;

        var spatialRef = pointFeature.PrimarySpatialReference!.Value;
        var isolatedNode = chart.GetIsolatedNode(spatialRef.Name);
        if (isolatedNode?.HasSoundings != true)
            yield break;

        foreach (var sounding in isolatedNode.Soundings)
        {
            var (lon, lat, depthMeters) = chart.ToDecimalValues(sounding);
            var (x, y) = SphericalMercator.FromLonLat(lon, lat);
            var point = new Point(x, y);

            var displayDepth = FormatDepth(depthMeters, depthUnit);

            var feature = new GeometryFeature(point);
            feature["ObjectCode"] = S57ObjectCode.SOUNDG;
            feature.Styles.Add(new LabelStyle
            {
                BackColor = null,
                Text = displayDepth,
                ForeColor = new Color(120, 120, 140),
                Font = new Font { Size = 12 },
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center
            });
            yield return feature;
        }
    }

    private const double MetersToFeet = 3.2808399;
    private const double MetersToFathoms = 0.5468066;

    private static string FormatDepth(double depthMeters, DepthUnit unit)
    {
        return unit switch
        {
            DepthUnit.Meters => depthMeters.ToString("0.0"),
            DepthUnit.Fathoms => FormatFathoms(depthMeters * MetersToFeet),
            _ => ((int)(depthMeters * MetersToFeet)).ToString(),
        };
    }

    private static string FormatFathoms(double totalFeet)
    {
        int wholeFeet = (int)totalFeet;
        int fathoms = wholeFeet / 6;
        int remainingFeet = wholeFeet % 6;

        // Unicode subscript digits: ₀₁₂₃₄₅
        const string subscriptDigits = "₀₁₂₃₄₅";

        return remainingFeet == 0
            ? fathoms.ToString()
            : $"{fathoms}{subscriptDigits[remainingFeet]}";
    }

    private static Point? CreatePointFromPointFeature(S57Chart chart, S57PointFeature pointFeature)
    {
        if (!pointFeature.HasSpatialReferences)
            return null;

        var spatialRef = pointFeature.PrimarySpatialReference!.Value;

        // Try isolated node first
        var isolatedNode = chart.GetIsolatedNode(spatialRef.Name);
        if (isolatedNode?.HasPosition == true)
        {
            var (lon, lat) = chart.ToDecimalDegrees(isolatedNode.Position!.Value);
            var (x, y) = SphericalMercator.FromLonLat(lon, lat);
            return new Point(x, y);
        }

        // Try connected node
        var connectedNode = chart.GetConnectedNode(spatialRef.Name);
        if (connectedNode != null)
        {
            var (lon, lat) = chart.ToDecimalDegrees(connectedNode.Position);
            var (x, y) = SphericalMercator.FromLonLat(lon, lat);
            return new Point(x, y);
        }

        return null;
    }

    private static Geometry? CreateLineStringFromLineFeature(S57Chart chart, S57LineFeature lineFeature)
        => S57LineGeometryBuilder.CreateLineStringFromLineFeature(chart, lineFeature);

    private static Geometry? CreatePolygonFromAreaFeature(S57Chart chart, S57AreaFeature areaFeature)
        => S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, areaFeature);

    // S-57 attribute codes for depth range values
    private const int DRVAL1 = 87; // Depth Range Value 1 (minimum depth, meters)
    private const int DRVAL2 = 88; // Depth Range Value 2 (maximum depth, meters)

    /// <summary>
    /// Creates a depth-band style for a DEPARE feature based on its DRVAL1/DRVAL2 attributes.
    /// Returns null for "deep" areas (>10m) which should not be displayed.
    /// </summary>
    private static IStyle? CreateDepthAreaStyle(S57AreaFeature areaFeature)
    {
        // Use DRVAL1 (minimum depth) to classify the band.
        // DRVAL1 is in meters; if missing, fall back to default area style.
        var drval1Str = areaFeature.GetAttributeValue(DRVAL1);
        if (drval1Str == null || !double.TryParse(drval1Str, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var minDepth))
        {
            return CreateAreaStyle(S57ObjectCode.DEPARE);
        }

        // Drying: 0-2m (sage green)
        if (minDepth < 2)
        {
            return new VectorStyle
            {
                Fill = new Brush(new Color(180, 210, 170, 130)),  // sage green
                Outline = new Pen(new Color(130, 170, 120, 150), 1),
            };
        }

        // Shallow: 2-5m (baby blue)
        if (minDepth < 5)
        {
            return new VectorStyle
            {
                Fill = new Brush(new Color(170, 210, 240, 120)),  // baby blue
                Outline = new Pen(new Color(100, 170, 220, 150), 1),
            };
        }

        // Safe: 5-10m (gray-blue)
        if (minDepth < 10)
        {
            return new VectorStyle
            {
                Fill = new Brush(new Color(160, 180, 200, 100)),  // gray-blue
                Outline = new Pen(new Color(120, 140, 170, 130), 1),
            };
        }

        // Deep: >10m — not displayed
        return null;
    }

    private static IStyle CreatePointStyle(S57ObjectCode objectCode)
    {
        if (objectCode == S57ObjectCode.LNDARE)
        {
            return new LabelStyle
            {
                BackColor = null,
                Text = "*",
                ForeColor = Color.Black,
                Font = new Font { Size = 14 },
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
            };
        }

        // Basic point styling - can be expanded based on object codes
        return new VectorStyle
        {
            Fill = new Brush(new Color(255, 0, 0, 200)),
            Outline = new Pen(Color.Black, 1)
        };
    }

    private static VectorStyle CreateLineStyle(S57ObjectCode objectCode)
    {
        // Traffic boundaries: darker purple, thicker, dashed
        if (objectCode is S57ObjectCode.TSSBND or S57ObjectCode.TSELNE)
        {
            return new VectorStyle
            {
                Line = new Pen(new Color(180, 100, 200, 200), 3)
                {
                    PenStyle = PenStyle.Dash,
                },
                Outline = null,
            };
        }

        // Different colors for different object types
        var (color, width) = objectCode switch
        {
            S57ObjectCode.DEPCNT => (new Color(0, 100, 200, 200), 1),
            S57ObjectCode.COALNE => (new Color(0, 0, 0, 255), 1),
            S57ObjectCode.SLCONS => (new Color(100, 100, 100, 255), 1),
            S57ObjectCode.BRIDGE => (new Color(80, 80, 80, 255), 2),
            S57ObjectCode.CBLOHD => (new Color(160, 0, 160, 200), 1),
            S57ObjectCode.CBLSUB => (new Color(160, 0, 160, 150), 1),
            S57ObjectCode.PIPSOL => (new Color(0, 160, 0, 180), 1),
            S57ObjectCode.FERYRT => (new Color(160, 0, 160, 180), 1),
            S57ObjectCode.NAVLNE => (new Color(200, 0, 200, 180), 1),
            S57ObjectCode.RECTRC => (new Color(200, 0, 200, 200), 1),
            S57ObjectCode.RIVERS => (new Color(0, 100, 200, 180), 1),
            S57ObjectCode.CANALS => (new Color(0, 100, 200, 180), 1),
            _ => (new Color(0, 0, 255, 150), 1)
        };

        return new VectorStyle
        {
            Line = new Pen(color, width)
        };
    }

    private static IStyle CreateAreaStyle(S57ObjectCode objectCode)
    {
        // Precautionary area: purple fill with dashed purple border
        if (objectCode == S57ObjectCode.PRCARE)
        {
            return new VectorStyle
            {
                Fill = new Brush(new Color(180, 100, 200, 40)),
                Outline = new Pen(new Color(180, 100, 200, 200), 2)
                {
                    PenStyle = PenStyle.Dash,
                },
            };
        }

        // Traffic separation zone: light purple fill, no border
        if (objectCode == S57ObjectCode.TSEZNE)
        {
            return new VectorStyle
            {
                Fill = new Brush(new Color(220, 200, 255, 60)),
                Outline = null,
            };
        }

        // Traffic separation lane: transparent, no border
        if (objectCode == S57ObjectCode.TSSLPT)
        {
            return new VectorStyle
            {
                Fill = null,
                Outline = null,
            };
        }

        // Different colors for different object types
        var (fillColor, outlineColor) = objectCode switch
        {
            S57ObjectCode.LNDARE => (new Color(200, 180, 140, 150), new Color(100, 80, 40, 200)),
            S57ObjectCode.DEPARE => (new Color(180, 220, 255, 100), new Color(0, 100, 200, 150)),
            S57ObjectCode.SEAARE => (new Color(200, 230, 255, 80), new Color(0, 100, 200, 100)),
            S57ObjectCode.BUAARE => (new Color(220, 180, 180, 150), new Color(150, 100, 100, 200)),
            S57ObjectCode.DRGARE => (new Color(180, 220, 255, 80), new Color(0, 100, 200, 120)),
            S57ObjectCode.LAKARE => (new Color(170, 210, 255, 120), new Color(0, 80, 180, 150)),
            S57ObjectCode.DOCARE => (new Color(180, 180, 200, 120), new Color(100, 100, 120, 180)),
            S57ObjectCode.ACHARE => (new Color(200, 200, 255, 80), new Color(100, 100, 200, 150)),
            S57ObjectCode.RESARE => (new Color(255, 200, 200, 80), new Color(200, 100, 100, 150)),
            S57ObjectCode.DMPGRD => (new Color(200, 200, 150, 80), new Color(150, 150, 80, 150)),
            S57ObjectCode.MIPARE => (new Color(255, 200, 200, 60), new Color(200, 80, 80, 120)),
            S57ObjectCode.CTNARE => (new Color(255, 230, 180, 80), new Color(200, 150, 50, 150)),
            S57ObjectCode.FAIRWY => (new Color(200, 220, 255, 60), new Color(100, 140, 200, 120)),
            S57ObjectCode.TSSRON => (new Color(220, 200, 255, 60), new Color(150, 100, 200, 120)),
            S57ObjectCode.TSSCRS => (new Color(220, 200, 255, 60), new Color(150, 100, 200, 120)),
            S57ObjectCode.CBLARE => (new Color(220, 180, 255, 60), new Color(160, 100, 200, 120)),
            S57ObjectCode.PIPARE => (new Color(180, 255, 180, 60), new Color(80, 160, 80, 120)),
            S57ObjectCode.UNSARE => (new Color(240, 240, 200, 80), new Color(180, 180, 100, 150)),
            _ => (new Color(200, 200, 200, 100), new Color(100, 100, 100, 150))
        };

        return new VectorStyle
        {
            Fill = new Brush(fillColor),
            Outline = new Pen(outlineColor, 1)
        };
    }
}
