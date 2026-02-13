using System;
using Avalonia.Controls;
using EncDotNet.ChartViewer.ViewModels;

namespace EncDotNet.ChartViewer.Views;

public partial class ManageChartsWindow : Window
{
    public ManageChartsWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is ManageChartsViewModel vm)
        {
            vm.RequestClose += () => Close();
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (DataContext is ManageChartsViewModel vm)
        {
            vm.Cancel();
        }
    }
}
