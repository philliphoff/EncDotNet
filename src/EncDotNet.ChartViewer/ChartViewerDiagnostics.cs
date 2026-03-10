using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace EncDotNet.ChartViewer;

internal static class ChartViewerDiagnostics
{
    public const string MeterName = "EncDotNet.ChartViewer";

    private static readonly string? MeterVersion = typeof(ChartViewerDiagnostics).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    public static readonly Meter Meter = new(MeterName, MeterVersion);

    public static readonly Counter<long> ChartsLoaded = Meter.CreateCounter<long>(
        "encdotnet.chartviewer.charts_loaded",
        description: "Number of charts loaded");

    public static readonly Histogram<double> ChartLoadDuration = Meter.CreateHistogram<double>(
        "encdotnet.chartviewer.chart_load_duration",
        unit: "ms",
        description: "Time to load and parse a chart");
}
