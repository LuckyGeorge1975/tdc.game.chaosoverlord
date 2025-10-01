using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ChaosOverlords.App.Views;

public partial class SectorView : UserControl
{
    public SectorView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
