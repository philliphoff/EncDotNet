using System.Reflection;
using Avalonia.Controls;

namespace EncDotNet.ChartViewer.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        var informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        VersionText.Text = informationalVersion is not null
            ? $"Version {informationalVersion}"
            : "Version unknown";
    }
}
