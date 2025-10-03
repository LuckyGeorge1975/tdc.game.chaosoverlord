using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ChaosOverlords.App.Views;

public partial class MapView : UserControl
{
    public MapView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}