using Avalonia;
using ReactiveUI.Avalonia;
using System;

namespace EncDotNet.ChartViewer;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        DiagnosticOptions.Parse(args);
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}