using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ChaosOverlords.App.Views;

public partial class TurnManagementView : UserControl
{
    public TurnManagementView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
