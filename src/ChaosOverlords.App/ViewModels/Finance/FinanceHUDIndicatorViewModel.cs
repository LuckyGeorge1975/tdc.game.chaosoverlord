using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.App.ViewModels.Finance;

/// <summary>
/// Compact HUD projection summary (Income, Expenses, Net) for the active player.
/// </summary>
public sealed partial class FinanceHUDIndicatorViewModel : ObservableObject
{
    private readonly IGameSession _session;
    private readonly IFinancePreviewService _previewService;

    [ObservableProperty] private int _income;
    [ObservableProperty] private int _expenses;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPositiveNet))]
    [NotifyPropertyChangedFor(nameof(HasNegativeNet))]
    private int _net;
    [ObservableProperty] private bool _hasData;

    public FinanceHUDIndicatorViewModel(IGameSession session, IFinancePreviewService previewService)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _previewService = previewService ?? throw new ArgumentNullException(nameof(previewService));
    }

    /// <summary>
    /// Optional callback wired externally (e.g. from TurnViewModel) to open the full financial dialog.
    /// Keeps this VM UI-service agnostic.
    /// </summary>
    public Action? ShowDialogAction { get; set; }

    public void Refresh()
    {
        if (!_session.IsInitialized)
        {
            HasData = false;
            Income = Expenses = Net = 0;
            return;
        }

        var state = _session.GameState;
        var playerId = state.PrimaryPlayerId; // assuming single local player context
        var projection = _previewService.BuildProjection(state, playerId);

        int income = 0, expense = 0;
        foreach (var cat in projection.CityCategories)
        {
            if (cat.Type == FinanceCategoryType.CashAdjustment) continue; // computed separately
            if (cat.Amount > 0) income += cat.Amount; else expense += cat.Amount; // expenses negative
        }

        Income = income;
        Expenses = expense; // already negative sum
        Net = income + expense;
        HasData = true;
    }

    [RelayCommand]
    private void ForceRefresh() => Refresh();

    public bool HasPositiveNet => Net > 0;
    public bool HasNegativeNet => Net < 0;

    [RelayCommand]
    private void OpenDialog() => ShowDialogAction?.Invoke();
}
