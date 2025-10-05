using ChaosOverlords.App.ViewModels.Finance;
using ChaosOverlords.App.Views.Finance;

namespace ChaosOverlords.App.Services;

/// <summary>
/// UI service responsible for constructing and displaying the city finance dialog.
/// Keeps view model code free of direct view construction.
/// </summary>
public sealed class CityFinancialDialogService : ICityFinancialDialogService
{
    private readonly CityFinancialDialogViewModel _dialogVm;

    public CityFinancialDialogService(CityFinancialDialogViewModel dialogVm)
    {
        _dialogVm = dialogVm ?? throw new ArgumentNullException(nameof(dialogVm));
    }

    public void ShowFinanceDialog()
    {
        _dialogVm.Refresh();
        var dialog = new CityFinancialDialogView
        {
            DataContext = _dialogVm
        };
        dialog.Show();
    }
}
