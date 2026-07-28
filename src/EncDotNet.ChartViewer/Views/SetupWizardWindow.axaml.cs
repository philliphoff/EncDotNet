using Avalonia.Controls;
using EncDotNet.ChartViewer.ViewModels;

namespace EncDotNet.ChartViewer.Views;

public partial class SetupWizardWindow : Window
{
    public SetupWizardWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is SetupWizardViewModel vm)
        {
            vm.RequestClose += () => Close();
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (DataContext is SetupWizardViewModel vm)
        {
            vm.Cancel();
        }
    }
}
