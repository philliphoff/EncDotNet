using System.Diagnostics.Metrics;
using System.Reflection;

namespace EncDotNet.ChartViewer;

internal static class ChartViewerDiagnostics
{
    public const string MeterName = "EncDotNet.ChartViewer";

    private static readonly string? MeterVersion = typeof(ChartViewerDiagnostics).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    public static readonly Meter Meter = new(MeterName, MeterVersion);

    // --- Chart loading ---

    public static readonly Counter<long> ChartsLoaded = Meter.CreateCounter<long>(
        "encdotnet.chartviewer.charts_loaded",
        description: "Number of charts successfully loaded");

    public static readonly Counter<long> ChartLoadErrors = Meter.CreateCounter<long>(
        "encdotnet.chartviewer.chart_load_errors",
        description: "Number of chart load failures");

    public static readonly UpDownCounter<long> ChartsLoading = Meter.CreateUpDownCounter<long>(
        "encdotnet.chartviewer.charts_loading",
        description: "Number of charts currently being loaded");

    public static readonly Histogram<double> ChartLoadDuration = Meter.CreateHistogram<double>(
        "encdotnet.chartviewer.chart_load_duration",
        unit: "ms",
        description: "Time to load and parse a chart");

    // --- Layer creation ---

    public static readonly Histogram<double> LayerCreationDuration = Meter.CreateHistogram<double>(
        "encdotnet.chartviewer.layer_creation_duration",
        unit: "ms",
        description: "Time to create all layers for a single chart");

    public static readonly Counter<long> LayersCreated = Meter.CreateCounter<long>(
        "encdotnet.chartviewer.layers_created",
        description: "Total number of layers created");

    public static readonly Histogram<long> FeaturesPerLayer = Meter.CreateHistogram<long>(
        "encdotnet.chartviewer.features_per_layer",
        unit: "{feature}",
        description: "Number of Mapsui features generated per layer");

    public static readonly Histogram<double> SingleLayerCreationDuration = Meter.CreateHistogram<double>(
        "encdotnet.chartviewer.single_layer_creation_duration",
        unit: "ms",
        description: "Time to create a single layer for one object code");

    // --- Geometry building ---

    public static readonly Histogram<double> AreaGeometryDuration = Meter.CreateHistogram<double>(
        "encdotnet.chartviewer.area_geometry_duration",
        unit: "ms",
        description: "Time spent building area geometries within a single layer");

    public static readonly Histogram<double> LineGeometryDuration = Meter.CreateHistogram<double>(
        "encdotnet.chartviewer.line_geometry_duration",
        unit: "ms",
        description: "Time spent building line geometries within a single layer");

    public static readonly Histogram<double> PointGeometryDuration = Meter.CreateHistogram<double>(
        "encdotnet.chartviewer.point_geometry_duration",
        unit: "ms",
        description: "Time spent building point geometries within a single layer");

    // --- Viewport evaluation ---

    public static readonly Histogram<double> ViewportEvaluationDuration = Meter.CreateHistogram<double>(
        "encdotnet.chartviewer.viewport_evaluation_duration",
        unit: "ms",
        description: "Time to evaluate viewport and load/unload charts");

    public static readonly Histogram<double> RecalculateMinVisibleDuration = Meter.CreateHistogram<double>(
        "encdotnet.chartviewer.recalculate_min_visible_duration",
        unit: "ms",
        description: "Time to recalculate MinVisible across all loaded charts");

    // --- Catalog ---

    public static readonly Histogram<double> CatalogLoadDuration = Meter.CreateHistogram<double>(
        "encdotnet.chartviewer.catalog_load_duration",
        unit: "ms",
        description: "Time to load and deserialize the chart catalog");
}
