using System.Reflection;
using Avalonia.Controls;

namespace EncDotNet.ChartViewer.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = version is not null ? $"Version {version}" : "Version unknown";
    }
}
