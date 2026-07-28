using System.Globalization;

namespace EncDotNet.ChartViewer;

/// <summary>
/// Parsed CLI diagnostic options. When <see cref="IsEnabled"/> is true, the app
/// navigates to the specified viewport, waits for charts to load, captures a
/// diagnostic snapshot (markdown + screenshot), writes them to the output path,
/// and exits.
/// </summary>
/// <remarks>
/// Usage:
///   --diagnostic --lon=-122.635 --lat=48.389 --resolution=17.55 --output=/tmp/diag
///
/// This writes two files:
///   /tmp/diag.md   — diagnostic markdown
///   /tmp/diag.png  — screenshot
/// </remarks>
internal sealed class DiagnosticOptions
{
    public static DiagnosticOptions? Current { get; private set; }

    public bool IsEnabled { get; init; }
    public double Longitude { get; init; }
    public double Latitude { get; init; }
    public double Resolution { get; init; }
    public string OutputPath { get; init; } = "";

    /// <summary>
    /// Parses diagnostic options from the command-line arguments.
    /// Returns <c>null</c> if <c>--diagnostic</c> is not present.
    /// Sets <see cref="Current"/> if parsed successfully.
    /// </summary>
    public static DiagnosticOptions? Parse(string[] args)
    {
        bool enabled = false;
        double lon = 0, lat = 0, resolution = 0;
        string output = "";

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--diagnostic")
            {
                enabled = true;
            }
            else if (args[i].StartsWith("--lon=", StringComparison.OrdinalIgnoreCase))
            {
                double.TryParse(args[i].AsSpan(6), NumberStyles.Float, CultureInfo.InvariantCulture, out lon);
            }
            else if (args[i].StartsWith("--lat=", StringComparison.OrdinalIgnoreCase))
            {
                double.TryParse(args[i].AsSpan(6), NumberStyles.Float, CultureInfo.InvariantCulture, out lat);
            }
            else if (args[i].StartsWith("--resolution=", StringComparison.OrdinalIgnoreCase))
            {
                double.TryParse(args[i].AsSpan(13), NumberStyles.Float, CultureInfo.InvariantCulture, out resolution);
            }
            else if (args[i].StartsWith("--output=", StringComparison.OrdinalIgnoreCase))
            {
                output = args[i][9..];
            }
        }

        if (!enabled)
            return null;

        var options = new DiagnosticOptions
        {
            IsEnabled = true,
            Longitude = lon,
            Latitude = lat,
            Resolution = resolution > 0 ? resolution : 10.0,
            OutputPath = string.IsNullOrEmpty(output)
                ? System.IO.Path.Combine(System.IO.Path.GetTempPath(), "EncDotNet-Diagnostics", "cli-diag")
                : output,
        };

        Current = options;
        return options;
    }
}
