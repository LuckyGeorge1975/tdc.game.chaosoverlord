using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.App.ViewModels.Finance;

public sealed partial class CityFinancialDialogViewModel : ObservableObject
{
    private readonly IGameSession _session;
    private readonly IFinancePreviewService _previewService;

    public ObservableCollection<FinanceCategoryRow> CityCategories { get; } = new();

    [ObservableProperty] private int _net;
    [ObservableProperty] private bool _hasData;
    [ObservableProperty] private string _playerName = string.Empty;

    public CityFinancialDialogViewModel(IGameSession session, IFinancePreviewService previewService)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _previewService = previewService ?? throw new ArgumentNullException(nameof(previewService));
    }

    public void Refresh()
    {
        CityCategories.Clear();
        if (!_session.IsInitialized)
        {
            Net = 0;
            PlayerName = string.Empty;
            HasData = false;
            return;
        }

        var state = _session.GameState;
        var projection = _previewService.BuildProjection(state, state.PrimaryPlayerId);
        PlayerName = projection.PlayerName;

        int net = 0;
        foreach (var cat in projection.CityCategories)
        {
            if (cat.Type != FinanceCategoryType.CashAdjustment)
                net += cat.Amount;
            CityCategories.Add(new FinanceCategoryRow(cat));
        }
        Net = net;
        HasData = true;
    }

    [RelayCommand]
    private void ForceRefresh() => Refresh();
}

public sealed class FinanceCategoryRow
{
    public FinanceCategoryRow(FinanceCategory category)
    {
        Type = category.Type;
        DisplayName = category.DisplayName;
        Amount = category.Amount;
        IsExpense = category.IsExpense;
        IsIncome = category.IsIncome;
    }

    public FinanceCategoryType Type { get; }
    public string DisplayName { get; }
    public int Amount { get; }
    public bool IsExpense { get; }
    public bool IsIncome { get; }
}
