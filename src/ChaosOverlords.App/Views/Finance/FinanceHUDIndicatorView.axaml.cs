using Avalonia.Controls;

namespace ChaosOverlords.App.Views.Finance;

public partial class FinanceHUDIndicatorView : UserControl
{
    public FinanceHUDIndicatorView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }
}
