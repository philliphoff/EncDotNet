using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using EncDotNet.S57;

namespace EncDotNet.ChartViewer.Views;

public partial class FeatureInfoWindow : Window
{
    private readonly string _clipboardText;

    public FeatureInfoWindow()
        : this("Unknown", null, "Unknown", "N/A", "N/A", null, null) { }

    public FeatureInfoWindow(
        string objectCode,
        int? objectCodeValue,
        string chartName,
        string latitude,
        string longitude,
        int? group,
        IReadOnlyList<S57AttributeValue>? attributes)
    {
        InitializeComponent();

        ObjectCodeText.Text = objectCodeValue.HasValue
            ? $"{objectCode} (OBJL {objectCodeValue.Value})"
            : objectCode;
        ChartNameText.Text = chartName;
        LatitudeText.Text = latitude;
        LongitudeText.Text = longitude;
        GroupText.Text = group?.ToString() ?? "N/A";

        if (attributes is { Count: > 0 })
        {
            AttributesHeader.IsVisible = true;
            AttributesText.IsVisible = true;
            var sb = new StringBuilder();
            foreach (var attr in attributes)
                sb.AppendLine($"ATTL {attr.AttributeCode} = {attr.Value}");
            AttributesText.Text = sb.ToString().TrimEnd();
        }

        _clipboardText = BuildClipboardText(objectCode, objectCodeValue, chartName, latitude, longitude, group, attributes);
    }

    private static string BuildClipboardText(
        string objectCode,
        int? objectCodeValue,
        string chartName,
        string latitude,
        string longitude,
        int? group,
        IReadOnlyList<S57AttributeValue>? attributes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("I clicked on an unrendered S-57 point feature (default red circle) in the chart viewer. Please add or update the rendering template for this feature.");
        sb.AppendLine();
        sb.AppendLine($"- **Object Code**: {objectCode} (OBJL {objectCodeValue?.ToString() ?? "?"})");
        sb.AppendLine($"- **Geometric Primitive**: Point");
        sb.AppendLine($"- **Group**: {group?.ToString() ?? "?"}");
        sb.AppendLine($"- **Chart**: {chartName}");
        sb.AppendLine($"- **Location**: {latitude}, {longitude}");

        if (attributes is { Count: > 0 })
        {
            sb.AppendLine();
            sb.AppendLine("Feature attributes:");
            foreach (var attr in attributes)
                sb.AppendLine($"- ATTL {attr.AttributeCode} = {attr.Value}");
        }

        return sb.ToString().TrimEnd();
    }

    private async void OnCopyClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(_clipboardText);
        }
    }

    private void OnOkClick(object? sender, RoutedEventArgs e) => Close();
}
