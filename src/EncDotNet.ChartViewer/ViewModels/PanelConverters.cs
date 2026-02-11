using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EncDotNet.ChartViewer.ViewModels;

/// <summary>
/// Converters used by panel collapse/expand buttons.
/// </summary>
public static class PanelConverters
{
    /// <summary>
    /// Converts a bool (isExpanded) to a collapse/expand label string.
    /// </summary>
    public static readonly IValueConverter CollapseLabel =
        new FuncValueConverter<bool, string>(isExpanded => isExpanded ? "Collapse" : "Expand");
}
