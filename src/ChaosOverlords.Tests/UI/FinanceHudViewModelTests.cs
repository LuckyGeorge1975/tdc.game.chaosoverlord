using ChaosOverlords.App.ViewModels.Finance;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Tests.UI;

public sealed class FinanceHudViewModelTests
{
    [Fact]
    public void Refresh_NoSessionInitialized_SetsHasDataFalse()
    {
        var session = new StubSession(false);
        var service = new StubFinancePreviewService();
        var vm = new FinanceHUDIndicatorViewModel(session, service);
        vm.Refresh();
        Assert.False(vm.HasData);
    }

    [Fact]
    public void Refresh_ComputesIncomeExpenseNet()
    {
        var playerId = Guid.NewGuid();
        var session = new StubSession(true, playerId, "Player");
        var projection = new FinanceProjection(playerId, "Player", new[]
        {
            new FinanceCategory(FinanceCategoryType.Upkeep, "Upkeep", -10),
            new FinanceCategory(FinanceCategoryType.SectorTax, "Sector Tax", 25),
            new FinanceCategory(FinanceCategoryType.CashAdjustment, "Cash Adjustment", 15)
        }, Array.Empty<FinanceSectorProjection>());
        var service = new StubFinancePreviewService(projection);
        var vm = new FinanceHUDIndicatorViewModel(session, service);
        vm.Refresh();
        Assert.True(vm.HasData);
        Assert.Equal(25, vm.Income);
        Assert.Equal(-10, vm.Expenses);
        Assert.Equal(15, vm.Net);
    }

    [Fact]
    public void OpenDialogCommand_InvokesShowDialogAction_WhenSet()
    {
        var session = new StubSession(false); // state irrelevant for this test
        var service = new StubFinancePreviewService();
        var vm = new FinanceHUDIndicatorViewModel(session, service);
        bool invoked = false;
        vm.ShowDialogAction = () => invoked = true;
        vm.OpenDialogCommand.Execute(null);
        Assert.True(invoked);
    }

    internal sealed class StubSession : IGameSession
    {
        public bool IsInitialized { get; }
    public Core.Domain.Scenario.ScenarioConfig Scenario { get; } = new() { Type = Core.Domain.Scenario.ScenarioType.KillEmAll, Name = "Test" };
        public GameState GameState { get; }
        public GameStateManager Manager => throw new NotImplementedException();

        public StubSession(bool init, Guid? primaryPlayerId = null, string? playerName = null)
        {
            IsInitialized = init;
            if (init && primaryPlayerId.HasValue && primaryPlayerId.Value != Guid.Empty)
            {
                var player = new Core.Domain.Players.Player(primaryPlayerId.Value, playerName ?? "Player");
                var game = new Game(new[] { player }, Array.Empty<Sector>(), null, null);
                GameState = new GameState(game, new Core.Domain.Scenario.ScenarioConfig { Type = Core.Domain.Scenario.ScenarioType.KillEmAll, Name = "Test" }, new[] { player });
            }
            else
            {
                // Minimal placeholder (uninitialized path code shouldn't consume GameState)
                var player = new Core.Domain.Players.Player(Guid.NewGuid(), "P");
                var game = new Game(new[] { player }, Array.Empty<Sector>(), null, null);
                GameState = new GameState(game, new Core.Domain.Scenario.ScenarioConfig { Type = Core.Domain.Scenario.ScenarioType.KillEmAll, Name = "Test" }, new[] { player });
            }
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    internal sealed class StubFinancePreviewService : IFinancePreviewService
    {
        private readonly FinanceProjection? _projection;
        public StubFinancePreviewService(FinanceProjection? projection = null) => _projection = projection;
        public FinanceProjection BuildProjection(GameState gameState, Guid playerId)
        {
            if (_projection is not null) return _projection;
            return new FinanceProjection(playerId, "Player", new []
            {
                new FinanceCategory(FinanceCategoryType.Upkeep, "Upkeep", -5),
                new FinanceCategory(FinanceCategoryType.CashAdjustment, "Cash Adjustment", -5)
            }, Array.Empty<FinanceSectorProjection>());
        }
    }
}
