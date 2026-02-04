using Avalonia.Controls;
using Mapsui.Tiling;

namespace EncDotNet.ChartViewer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        MyMapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
    }
}