using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EncDotNet.ChartViewer.Charts;
using EncDotNet.ChartViewer.ViewModels;
using EncDotNet.ChartViewer.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace EncDotNet.ChartViewer;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnAboutClicked(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog(desktop.MainWindow);
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Start OpenTelemetry hosted services (metric export, etc.)
        // which are not automatically started outside of a generic host.
        foreach (var hostedService in Services.GetServices<IHostedService>())
        {
            hostedService.StartAsync(CancellationToken.None);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService("EncDotNet.ChartViewer");

        services.AddLogging(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.AddConsoleExporter();
                options.AddOtlpExporter();
            });
        });

        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.SetResourceBuilder(resourceBuilder);
                metrics.AddMeter("EncDotNet.ChartViewer");
                metrics.AddConsoleExporter();
                metrics.AddOtlpExporter();
            });

        services.AddSingleton<IChartCatalogSource>(sp => new FileSystemChartCatalogSource(
            AppDataPaths.ChartIndexPath,
            sp.GetRequiredService<ILogger<FileSystemChartCatalogSource>>()));
        services.AddSingleton<IChartSource>(sp => new CachedChartSource(
            sp.GetRequiredService<IChartCatalogSource>()));
        services.AddSingleton<IChartPackageManager, NoaaChartPackageManager>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<SetupWizardViewModel>();
        services.AddTransient<ManageChartsViewModel>();
    }
}