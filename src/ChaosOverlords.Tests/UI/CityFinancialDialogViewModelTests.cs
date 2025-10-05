using ChaosOverlords.App.ViewModels.Finance;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Tests.UI;

public sealed class CityFinancialDialogViewModelTests
{
    [Fact]
    public void Refresh_PopulatesCategoriesAndNet()
    {
        var playerId = Guid.NewGuid();
        var session = new FinanceHudViewModelTests.StubSession(true, playerId, "Alice");
        var projection = new FinanceProjection(playerId, "Alice", new[]
        {
            new FinanceCategory(FinanceCategoryType.Upkeep, "Upkeep", -7),
            new FinanceCategory(FinanceCategoryType.SectorTax, "Sector Tax", 20),
            new FinanceCategory(FinanceCategoryType.CashAdjustment, "Cash Adjustment", 13)
        }, Array.Empty<FinanceSectorProjection>());
        var service = new FinanceHudViewModelTests.StubFinancePreviewService(projection);
        var vm = new CityFinancialDialogViewModel(session, service);
        vm.Refresh();
        Assert.True(vm.HasData);
        Assert.Equal(13, vm.Net);
        Assert.Equal("Alice", vm.PlayerName);
        Assert.Equal(3, vm.CityCategories.Count);
    }
}
