using System.Collections.ObjectModel;
using EncDotNet.ChartViewer.Models;

namespace EncDotNet.ChartViewer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    /// <summary>
    /// Gets the collection of toggleable chart feature categories.
    /// </summary>
    public ObservableCollection<ChartFeatureViewModel> FeatureCategories { get; } = new();

    public MainWindowViewModel()
    {
        foreach (var category in S57FeatureCategory.All)
        {
            FeatureCategories.Add(new ChartFeatureViewModel(category));
        }
    }
}