using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EncDotNet.ChartViewer.Catalogs;
using EncDotNet.ChartViewer.ViewModels;
using EncDotNet.ChartViewer.Views;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddSingleton<ICatalogSource>(_ => new FileSystemCatalogSource(AppDataPaths.ChartIndexPath));
        services.AddSingleton<IChartPackageManager, NoaaChartPackageManager>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<SetupWizardViewModel>();
        services.AddTransient<ManageChartsViewModel>();
    }
}