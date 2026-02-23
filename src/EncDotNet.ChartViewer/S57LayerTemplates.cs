using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using EncDotNet.Enc;
using EncDotNet.Enc.Charts;
using EncDotNet.ChartViewer.Models;
using Mapsui;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using NetTopologySuite.Geometries;

namespace EncDotNet.ChartViewer;

/// <summary>
/// Registry of <see cref="S57LayerTemplate"/> instances keyed by S-57 object code.
/// </summary>
internal static class S57LayerTemplates
{
    private const string IconPrefix = "embedded://EncDotNet.ChartViewer.Assets.ChartSymbols.NChart-Symbol-INT-";
    private const string UnderwaterRockIconSource = "embedded://EncDotNet.ChartViewer.Assets.ChartSymbols.NChart-Symbol-INT-Rock-Underwater.svg";

    // S-57 attribute codes
    private const int BCNSHP = 3;   // Beacon shape
    private const int BOYSHP = 4;   // Buoy shape
    private const int COLOUR = 75;  // Colour
    private const int TOPSHP = 171; // Topmark shape
    private const int DRVAL1 = 87;  // Depth Range Value 1 (minimum depth, meters)

    private const double MetersToFeet = 3.2808399;
    private const double SoundingMaxResolution = 10;

    /// <summary>
    /// The default template applied when no object-code-specific template is registered.
    /// </summary>
    public static S57LayerTemplate Default { get; } = new()
    {
        Area = S57LayerTemplate.AreaStyle(
            new Color(200, 200, 200, 100),
            new Color(100, 100, 100, 150)),
        Line = S57LayerTemplate.LineStyle(
            new Color(0, 0, 255, 150), 1),
        Point = S57LayerTemplate.PointStyle(new VectorStyle
        {
            Fill = new Brush(new Color(255, 0, 0, 200)),
            Outline = new Pen(Color.Black, 1),
        }),
    };

    private static readonly FrozenDictionary<S57ObjectCode, S57LayerTemplate> Templates = BuildTemplates();

    /// <summary>
    /// Returns the template for the given object code, or <see cref="Default"/> if none is registered.
    /// </summary>
    public static S57LayerTemplate GetTemplate(S57ObjectCode objectCode)
        => Templates.GetValueOrDefault(objectCode, Default);

    private static FrozenDictionary<S57ObjectCode, S57LayerTemplate> BuildTemplates()
    {
        // Shared templates for groups of related object codes
        var buoyTemplate = new S57LayerTemplate
        {
            Point = CreateBuoyFeatures,
        };

        var beaconTemplate = new S57LayerTemplate
        {
            Point = CreateBeaconFeatures,
        };

        var tssLineDashed = new S57LayerTemplate
        {
            Line = S57LayerTemplate.LineStyle(new VectorStyle
            {
                Line = new Pen(new Color(180, 100, 200, 200), 3) { PenStyle = PenStyle.Dash },
                Outline = null,
            }),
        };

        var tssRoundaboutCrossing = new S57LayerTemplate
        {
            Area = S57LayerTemplate.AreaStyle(
                new Color(220, 200, 255, 60),
                new Color(150, 100, 200, 120)),
        };

        var dict = new Dictionary<S57ObjectCode, S57LayerTemplate>
        {
            // --- Hydrographic ---

            [S57ObjectCode.COALNE] = new()
            {
                Line = S57LayerTemplate.LineStyle(new Color(0, 0, 0, 255), 1),
            },
            [S57ObjectCode.DEPCNT] = new()
            {
                Line = S57LayerTemplate.LineStyle(new Color(0, 100, 200, 200), 1),
            },
            [S57ObjectCode.DEPARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(CreateDepareStyle),
            },
            [S57ObjectCode.SOUNDG] = new()
            {
                Point = CreateSoundingFeatures,
                MaxVisible = SoundingMaxResolution,
            },
            [S57ObjectCode.SEAARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(200, 230, 255, 80),
                    new Color(0, 100, 200, 100)),
            },
            [S57ObjectCode.DRGARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(180, 220, 255, 80),
                    new Color(0, 100, 200, 120)),
            },
            [S57ObjectCode.LAKARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(170, 210, 255, 120),
                    new Color(0, 80, 180, 150)),
            },
            [S57ObjectCode.RIVERS] = new()
            {
                Line = S57LayerTemplate.LineStyle(new Color(0, 100, 200, 180), 1),
            },
            [S57ObjectCode.CANALS] = new()
            {
                Line = S57LayerTemplate.LineStyle(new Color(0, 100, 200, 180), 1),
            },
            [S57ObjectCode.UNSARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(240, 240, 200, 80),
                    new Color(180, 180, 100, 150)),
            },

            // --- Land ---

            [S57ObjectCode.LNDARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(200, 180, 140, 150),
                    new Color(100, 80, 40, 200)),
                Point = S57LayerTemplate.PointStyle(new LabelStyle
                {
                    BackColor = null,
                    Text = "*",
                    ForeColor = Color.Black,
                    Font = new Font { Size = 14 },
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                    VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
                }),
            },
            [S57ObjectCode.BUAARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(220, 180, 180, 150),
                    new Color(150, 100, 100, 200)),
            },

            // --- Structures ---

            [S57ObjectCode.SLCONS] = new()
            {
                Line = S57LayerTemplate.LineStyle(new Color(100, 100, 100, 255), 1),
            },
            [S57ObjectCode.BRIDGE] = new()
            {
                Line = S57LayerTemplate.LineStyle(new Color(80, 80, 80, 255), 2),
            },
            [S57ObjectCode.DOCARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(180, 180, 200, 120),
                    new Color(100, 100, 120, 180)),
            },

            // --- Navigation aids ---

            [S57ObjectCode.BOYCAR] = buoyTemplate,
            [S57ObjectCode.BOYINB] = buoyTemplate,
            [S57ObjectCode.BOYISD] = buoyTemplate,
            [S57ObjectCode.BOYLAT] = buoyTemplate,
            [S57ObjectCode.BOYSAW] = buoyTemplate,
            [S57ObjectCode.BOYSPP] = buoyTemplate,

            [S57ObjectCode.BCNCAR] = beaconTemplate,
            [S57ObjectCode.BCNISD] = beaconTemplate,
            [S57ObjectCode.BCNLAT] = beaconTemplate,
            [S57ObjectCode.BCNSAW] = beaconTemplate,
            [S57ObjectCode.BCNSPP] = beaconTemplate,

            // --- Hazards ---

            [S57ObjectCode.UWTROC] = new()
            {
                Point = S57LayerTemplate.ImagePointStyle(UnderwaterRockIconSource, 0.64),
            },

            // --- Cables & pipelines ---

            [S57ObjectCode.CBLOHD] = new()
            {
                Line = S57LayerTemplate.LineStyle(new Color(160, 0, 160, 200), 1),
            },
            [S57ObjectCode.CBLSUB] = new()
            {
                Line = S57LayerTemplate.LineStyle(new Color(160, 0, 160, 150), 1),
            },
            [S57ObjectCode.CBLARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(220, 180, 255, 60),
                    new Color(160, 100, 200, 120)),
            },
            [S57ObjectCode.PIPSOL] = new()
            {
                Line = S57LayerTemplate.LineStyle(new Color(0, 160, 0, 180), 1),
            },
            [S57ObjectCode.PIPARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(180, 255, 180, 60),
                    new Color(80, 160, 80, 120)),
            },

            // --- Navigation routing ---

            [S57ObjectCode.FERYRT] = new()
            {
                Line = S57LayerTemplate.LineStyle(new Color(160, 0, 160, 180), 1),
            },
            [S57ObjectCode.NAVLNE] = new()
            {
                Line = S57LayerTemplate.LineStyle(new Color(200, 0, 200, 180), 1),
            },
            [S57ObjectCode.RECTRC] = new()
            {
                Line = S57LayerTemplate.LineStyle(new Color(200, 0, 200, 200), 1),
            },
            [S57ObjectCode.TSSBND] = tssLineDashed,
            [S57ObjectCode.TSELNE] = tssLineDashed,
            [S57ObjectCode.TSEZNE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(new VectorStyle
                {
                    Fill = new Brush(new Color(220, 200, 255, 60)),
                    Outline = null,
                }),
            },
            [S57ObjectCode.TSSLPT] = new()
            {
                Area = S57LayerTemplate.AreaStyle(new VectorStyle
                {
                    Fill = null,
                    Outline = null,
                }),
            },
            [S57ObjectCode.TSSRON] = tssRoundaboutCrossing,
            [S57ObjectCode.TSSCRS] = tssRoundaboutCrossing,
            [S57ObjectCode.PRCARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(new VectorStyle
                {
                    Fill = new Brush(new Color(180, 100, 200, 40)),
                    Outline = new Pen(new Color(180, 100, 200, 200), 2) { PenStyle = PenStyle.Dash },
                }),
            },
            [S57ObjectCode.FAIRWY] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(200, 220, 255, 60),
                    new Color(100, 140, 200, 120)),
            },

            // --- Regulated/restricted areas ---

            [S57ObjectCode.ACHARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(200, 200, 255, 80),
                    new Color(100, 100, 200, 150)),
            },
            [S57ObjectCode.RESARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(255, 200, 200, 80),
                    new Color(200, 100, 100, 150)),
            },
            [S57ObjectCode.DMPGRD] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(200, 200, 150, 80),
                    new Color(150, 150, 80, 150)),
            },
            [S57ObjectCode.MIPARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(255, 200, 200, 60),
                    new Color(200, 80, 80, 120)),
            },
            [S57ObjectCode.CTNARE] = new()
            {
                Area = S57LayerTemplate.AreaStyle(
                    new Color(255, 230, 180, 80),
                    new Color(200, 150, 50, 150)),
            },
        };

        return dict.ToFrozenDictionary();
    }

    // --- Custom handlers ---

    private static IStyle? CreateDepareStyle(S57AreaFeature feature)
    {
        var drval1Str = feature.GetAttributeValue(DRVAL1);
        if (drval1Str == null || !double.TryParse(drval1Str, NumberStyles.Float, CultureInfo.InvariantCulture, out var minDepth))
        {
            // Fallback: default DEPARE color
            return new VectorStyle
            {
                Fill = new Brush(new Color(180, 220, 255, 100)),
                Outline = new Pen(new Color(0, 100, 200, 150), 1),
            };
        }

        if (minDepth < 2)
        {
            return new VectorStyle
            {
                Fill = new Brush(new Color(180, 210, 170, 130)),
                Outline = new Pen(new Color(130, 170, 120, 150), 1),
            };
        }

        if (minDepth < 5)
        {
            return new VectorStyle
            {
                Fill = new Brush(new Color(170, 210, 240, 120)),
                Outline = new Pen(new Color(100, 170, 220, 150), 1),
            };
        }

        if (minDepth < 10)
        {
            return new VectorStyle
            {
                Fill = new Brush(new Color(160, 180, 200, 100)),
                Outline = new Pen(new Color(120, 140, 170, 130), 1),
            };
        }

        // Deep: >10m — not displayed
        return null;
    }

    private static IEnumerable<IFeature> CreateSoundingFeatures(S57Chart chart, S57PointFeature pointFeature, DepthUnit depthUnit)
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

            var feature = new GeometryFeature(new Point(x, y));
            feature["ObjectCode"] = S57ObjectCode.SOUNDG;
            feature.Styles.Add(new LabelStyle
            {
                BackColor = null,
                Text = FormatDepth(depthMeters, depthUnit),
                ForeColor = new Color(120, 120, 140),
                Font = new Font { Size = 12 },
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
            });
            yield return feature;
        }
    }

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

        const string subscriptDigits = "₀₁₂₃₄₅";

        return remainingFeet == 0
            ? fathoms.ToString()
            : $"{fathoms}{subscriptDigits[remainingFeet]}";
    }

    // --- Buoy icon selection ---

    private static IEnumerable<IFeature> CreateBuoyFeatures(S57Chart chart, S57PointFeature feature, DepthUnit _)
    {
        var iconSource = GetBuoyIconSource(chart, feature);
        if (iconSource == null)
        {
            // No matching icon — fall back to a conspicuous red circle.
            return S57LayerTemplate.CreatePointFeature(chart, feature, new VectorStyle
            {
                Fill = new Brush(new Color(255, 0, 0, 200)),
                Outline = new Pen(Color.Black, 1),
            });
        }

        var style = new ImageStyle { Image = new Image { Source = iconSource }, SymbolScale = 0.2 };
        return S57LayerTemplate.CreatePointFeature(chart, feature, style);
    }

    private static string? GetBuoyIconSource(S57Chart chart, S57PointFeature feature)
    {
        var shapeName = GetBuoyShapeName(feature);
        if (shapeName == null)
            return null;

        var colorName = GetColorName(feature);
        var topmarkName = GetTopmarkName(feature);
        var isLighted = HasRelatedLight(chart, feature);

        var lighted = isLighted ? "Lighted-" : "";
        var color = colorName != null ? $"-{colorName}" : "";
        var topmark = topmarkName != null ? $"-{topmarkName}TM" : "";

        return $"{IconPrefix}{lighted}{shapeName}Buoy{color}{topmark}.svg";
    }

    private static string? GetBuoyShapeName(S57PointFeature feature)
    {
        var boyshp = feature.GetAttributeValue(BOYSHP);
        if (boyshp == null) return null;

        return boyshp switch
        {
            "1" => "Conical",
            "2" => "Can",
            "3" => "Spherical",
            "4" => "Pillar",
            "5" => "Spar",
            "6" => "Barrel",
            _ => null,
        };
    }

    private static string? GetColorName(S57PointFeature feature)
    {
        var colour = feature.GetAttributeValue(COLOUR);
        if (colour == null) return null;

        // COLOUR can be comma-separated; use the first value.
        var commaIndex = colour.IndexOf(',');
        var firstColour = commaIndex >= 0 ? colour[..commaIndex] : colour;

        return firstColour switch
        {
            "3" => "Red",
            "4" => "Green",
            _ => null,
        };
    }

    private static string? GetTopmarkName(S57PointFeature feature)
    {
        var topshp = feature.GetAttributeValue(TOPSHP);
        if (topshp == null) return null;

        return topshp switch
        {
            "1" or "2" => "Conical",
            "3" or "4" => "Sphere",
            "5" => "Cylindrical",
            "7" or "8" => "XShape",
            _ => null,
        };
    }

    private static bool HasRelatedLight(S57Chart chart, S57PointFeature feature)
    {
        // In S-57, LIGHTS features are co-located on the same spatial node (isolated node)
        // as the buoy/beacon they illuminate, rather than linked via FFPT.
        if (feature.HasSpatialReferences)
        {
            var spatialName = feature.PrimarySpatialReference!.Value.Name;
            foreach (var colocated in chart.GetColocatedPointFeatures(spatialName))
            {
                if (colocated.ObjectCode == S57ObjectCode.LIGHTS)
                    return true;
            }
        }

        return false;
    }

    // --- Beacon icon selection ---

    private static IEnumerable<IFeature> CreateBeaconFeatures(S57Chart chart, S57PointFeature feature, DepthUnit _)
    {
        var iconSource = GetBeaconIconSource(chart, feature);
        if (iconSource == null)
        {
            return S57LayerTemplate.CreatePointFeature(chart, feature, new VectorStyle
            {
                Fill = new Brush(new Color(255, 0, 0, 200)),
                Outline = new Pen(Color.Black, 1),
            });
        }

        var style = new ImageStyle { Image = new Image { Source = iconSource }, SymbolScale = 0.2 };
        return S57LayerTemplate.CreatePointFeature(chart, feature, style);
    }

    private static string? GetBeaconIconSource(S57Chart chart, S57PointFeature feature)
    {
        var shapeName = GetBeaconShapeName(feature);
        var colorName = GetColorName(feature);
        var topmarkName = GetBeaconTopmarkName(feature);
        var isLighted = HasRelatedLight(chart, feature);

        // Standard (non-tower) beacon icons only include color when a topmark is also present.
        var isTower = shapeName.Length > 0;
        if (!isTower && topmarkName == null)
            colorName = null;

        var lighted = isLighted ? "Lighted-" : "";
        var color = colorName != null ? $"-{colorName}" : "";
        var topmark = topmarkName != null ? $"-{topmarkName}TM" : "";

        return $"{IconPrefix}{lighted}{shapeName}Beacon{color}{topmark}.svg";
    }

    private static string GetBeaconShapeName(S57PointFeature feature)
    {
        var bcnshp = feature.GetAttributeValue(BCNSHP);

        return bcnshp switch
        {
            "3" => "Tower",
            _ => "",
        };
    }

    private static string? GetBeaconTopmarkName(S57PointFeature feature)
    {
        var topshp = feature.GetAttributeValue(TOPSHP);
        if (topshp == null) return null;

        return topshp switch
        {
            "1" or "2" => "Conical",
            "5" => "Cylindrical",
            _ => null,
        };
    }
}
