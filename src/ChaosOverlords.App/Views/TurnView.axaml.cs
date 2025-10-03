using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ChaosOverlords.App.Views;

public partial class TurnView : UserControl
{
    public TurnView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}