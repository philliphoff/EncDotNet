namespace EncDotNet.ChartViewer.ViewModels;

/// <summary>
/// Represents a state group header in the chart list.
/// </summary>
public sealed class ChartGroupHeaderViewModel
{
    /// <summary>Gets the state name displayed as the group header.</summary>
    public string StateName { get; }

    /// <summary>Gets the number of charts in this group.</summary>
    public int ChartCount { get; }

    public ChartGroupHeaderViewModel(string stateName, int chartCount)
    {
        StateName = stateName;
        ChartCount = chartCount;
    }
}
