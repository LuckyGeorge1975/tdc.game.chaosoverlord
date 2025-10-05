using Avalonia.Controls;
using Avalonia.Interactivity;
using ChaosOverlords.App.ViewModels.Finance;

namespace ChaosOverlords.App.Views.Finance;

public partial class CityFinancialDialogView : Window
{
    public CityFinancialDialogView()
    {
        InitializeComponent();
        if (DataContext is CityFinancialDialogViewModel vm)
            vm.Refresh();
        Opened += (_, _) => (DataContext as CityFinancialDialogViewModel)?.Refresh();
    }

    private void InitializeComponent()
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
