using ChaosOverlords.App.ViewModels;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Actions;
using ChaosOverlords.Core.Domain.Game.Commands;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Domain.Game.Events;
using ChaosOverlords.Core.Domain.Game.Recruitment;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;
using ChaosOverlords.Core.Services.Messaging;

namespace ChaosOverlords.Tests.ViewModels;

public class TurnViewModelTests
{
    [Fact]
    public void EndTurnCommand_is_enabled_in_elimination_phase()
    {
        var controller = new TurnController();
        var session = new StubGameSession();
        var recruitmentService = new NoopRecruitmentService();
        var eventLog = new TurnEventLog();
        var eventWriter = new NullEventWriter();
        var commandQueueService = new DummyCommandQueueService();
        var financePreviewService = new StubFinancePreviewService();

        using var viewModel = CreateViewModel(controller, eventLog, session, recruitmentService, eventWriter,
            commandQueueService, financePreviewService);

        Assert.False(viewModel.EndTurnCommand.CanExecute(null));

        viewModel.StartTurnCommand.Execute(null);

        // The maximum number of phase advances is set to 32 as a safety guard to prevent infinite loops.
        // This should be greater than or equal to the number of phases in the turn state machine.
        var maxPhaseAdvances = 32;
        var guard = maxPhaseAdvances;
        while (viewModel.CurrentPhase != TurnPhase.Elimination && guard-- > 0)
        {
            if (!viewModel.AdvancePhaseCommand.CanExecute(null)) break;

            viewModel.AdvancePhaseCommand.Execute(null);
        }

        Assert.True(guard > 0, $"Failed to reach elimination phase within {maxPhaseAdvances} advances.");
        Assert.Equal(TurnPhase.Elimination, viewModel.CurrentPhase);
        Assert.True(viewModel.EndTurnCommand.CanExecute(null));
    }

    [Fact]
    public void HireCommand_HiresOptionAndLogsEvent()
    {
        var controller = new TurnController();
        var session = new StubGameSession();
        var recruitmentService = new RecordingRecruitmentService();
        var eventLog = new TurnEventLog();
        var eventWriter = new RecordingEventWriter(eventLog);
        var commandQueueService = new DummyCommandQueueService();
        var financePreviewService = new StubFinancePreviewService();

        using var viewModel = CreateViewModel(controller, eventLog, session, recruitmentService, eventWriter,
            commandQueueService, financePreviewService);

        controller.StartTurn();
        AdvanceToPhase(controller, TurnPhase.Hire);

        Assert.True(viewModel.IsRecruitmentPanelVisible);
        var option = viewModel.RecruitmentOptions.First();
        Assert.NotNull(viewModel.SelectedSector);
        Assert.True(viewModel.HireCommand.CanExecute(option));

        viewModel.HireCommand.Execute(option);

        var hireCall = Assert.Single(recruitmentService.HireCalls);
        Assert.Equal(session.GameState.PrimaryPlayerId, hireCall.PlayerId);
        Assert.Equal(option.OptionId, hireCall.OptionId);
        Assert.Equal(viewModel.SelectedSector!.SectorId, hireCall.SectorId);

        var updatedOption = viewModel.RecruitmentOptions.First(o => o.OptionId == option.OptionId);
        Assert.Equal(RecruitmentOptionState.Hired, updatedOption.State);
        Assert.True(viewModel.HasRecruitmentStatusMessage);
        Assert.Contains("Hired", viewModel.RecruitmentStatusMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(eventWriter.Events,
            e => e.Type == TurnEventType.Recruitment &&
                 e.Description.Contains("hired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DeclineCommand_DeclinesOptionAndUpdatesMessage()
    {
        var controller = new TurnController();
        var session = new StubGameSession();
        var recruitmentService = new RecordingRecruitmentService();
        var eventLog = new TurnEventLog();
        var eventWriter = new RecordingEventWriter(eventLog);
        var commandQueueService = new DummyCommandQueueService();
        var financePreviewService = new StubFinancePreviewService();

        using var viewModel = CreateViewModel(controller, eventLog, session, recruitmentService, eventWriter,
            commandQueueService, financePreviewService);

        controller.StartTurn();
        AdvanceToPhase(controller, TurnPhase.Hire);

        var option = viewModel.RecruitmentOptions[1];
        Assert.True(viewModel.DeclineCommand.CanExecute(option));

        viewModel.DeclineCommand.Execute(option);

        var declineCall = Assert.Single(recruitmentService.DeclineCalls);
        Assert.Equal(option.OptionId, declineCall.OptionId);

        var updatedOption = viewModel.RecruitmentOptions.First(o => o.OptionId == option.OptionId);
        Assert.Equal(RecruitmentOptionState.Declined, updatedOption.State);
        Assert.True(viewModel.HasRecruitmentStatusMessage);
        Assert.Contains("Declined", viewModel.RecruitmentStatusMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(eventWriter.Events,
            e => e.Type == TurnEventType.Recruitment &&
                 e.Description.Contains("declined", StringComparison.OrdinalIgnoreCase));
        Assert.False(viewModel.HireCommand.CanExecute(option));
    }

    [Fact]
    public void FinancePreview_PopulatesCityAndSectorCollections()
    {
        var controller = new TurnController();
        var session = new StubGameSession();
        var recruitmentService = new NoopRecruitmentService();
        var eventLog = new TurnEventLog();
        var eventWriter = new NullEventWriter();
        var commandQueueService = new DummyCommandQueueService();
        var financePreviewService = new StubFinancePreviewService();

        using var viewModel = CreateViewModel(controller, eventLog, session, recruitmentService, eventWriter,
            commandQueueService, financePreviewService);

        controller.StartTurn();
        AdvanceToPhase(controller, TurnPhase.Command);

        Assert.True(viewModel.IsFinancePreviewVisible);
        Assert.NotEmpty(viewModel.CityFinanceCategories);
        Assert.NotEmpty(viewModel.SectorFinance);
        Assert.Contains(viewModel.CityFinanceCategories, category => category.DisplayName == "Upkeep");
        Assert.Contains(viewModel.SectorFinance,
            sector => sector.DisplayName.StartsWith("Sector", StringComparison.OrdinalIgnoreCase));
    }

    private static TurnViewModel CreateViewModel(
        ITurnController controller,
        ITurnEventLog eventLog,
        IGameSession session,
        IRecruitmentService recruitmentService,
        ITurnEventWriter eventWriter,
        ICommandQueueService commandQueueService,
        IFinancePreviewService financePreviewService)
    {
        var messageHub = new MessageHub();
        var logPathProvider = new TestLogPathProvider();
        var researchService = new ResearchService();
        return new TurnViewModel(controller, eventLog, session, recruitmentService, eventWriter, commandQueueService,
            financePreviewService, researchService, messageHub, logPathProvider);
    }

    private static SiteData CreateSite(string name)
    {
        return new SiteData { Name = name, Cash = 2, Tolerance = 1 };
    }

    private static void AdvanceToPhase(TurnController controller, TurnPhase targetPhase)
    {
        var guard = 32;
        while (controller.CurrentPhase != targetPhase && guard-- > 0)
        {
            if (!controller.CanAdvancePhase) break;

            controller.AdvancePhase();
        }
    }

    private sealed class TestLogPathProvider : ILogPathProvider
    {
        public string GetLogDirectory()
        {
            return Path.Combine(Path.GetTempPath(), "co_test_logs");
        }
    }

    private sealed class StubGameSession : IGameSession
    {
        public StubGameSession()
        {
            var player = new Player(Guid.NewGuid(), "Player One", 100);
            var sector = new Sector("A1", CreateSite("A1 Hub"), player.Id);
            var game = new Game(new IPlayer[] { player }, new[] { sector });
            var scenario = new ScenarioConfig
            {
                Type = ScenarioType.KillEmAll,
                Name = "Stub",
                Players = new List<ScenarioPlayerConfig>
                {
                    new()
                    {
                        Name = player.Name,
                        Kind = PlayerKind.Human,
                        StartingCash = 100,
                        HeadquartersSectorId = sector.Id,
                        StartingGangName = "Hackers"
                    }
                }
            };

            GameState = new GameState(game, scenario, new List<IPlayer> { player }, 0, 1);
            Manager = new GameStateManager(GameState);
            IsInitialized = true;
        }

        public bool IsInitialized { get; private set; }

        public ScenarioConfig Scenario => GameState.Scenario;

        public GameState GameState { get; }

        public GameStateManager Manager { get; }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }
    }

    private sealed class NoopRecruitmentService : IRecruitmentService
    {
        public RecruitmentPoolSnapshot EnsurePool(GameState gameState, Guid playerId, int turnNumber)
        {
            return new RecruitmentPoolSnapshot(playerId, gameState.Game.GetPlayer(playerId).Name,
                Array.Empty<RecruitmentOptionSnapshot>());
        }

        public IReadOnlyList<RecruitmentRefreshResult> RefreshPools(GameState gameState, int turnNumber)
        {
            return Array.Empty<RecruitmentRefreshResult>();
        }

        public RecruitmentHireResult Hire(GameState gameState, Guid playerId, Guid optionId, string sectorId,
            int turnNumber)
        {
            var snapshot = EnsurePool(gameState, playerId, turnNumber);
            return new RecruitmentHireResult(RecruitmentActionStatus.InvalidOption, snapshot, null, null, sectorId,
                "No recruitment in test stub.");
        }

        public RecruitmentDeclineResult Decline(GameState gameState, Guid playerId, Guid optionId, int turnNumber)
        {
            var snapshot = EnsurePool(gameState, playerId, turnNumber);
            return new RecruitmentDeclineResult(RecruitmentActionStatus.InvalidOption, snapshot, null,
                "No recruitment in test stub.");
        }
    }

    private sealed class NullEventWriter : ITurnEventWriter
    {
        public void Write(int turnNumber, TurnPhase phase, TurnEventType type, string description,
            CommandPhase? commandPhase = null)
        {
        }

        public void WriteEconomy(int turnNumber, TurnPhase phase, PlayerEconomySnapshot snapshot)
        {
        }

        public void WriteAction(ActionResult result)
        {
        }
    }

    private sealed class DummyCommandQueueService : ICommandQueueService
    {
        public CommandQueueSnapshot GetQueue(GameState gameState, Guid playerId)
        {
            return new CommandQueueSnapshot(playerId, Array.Empty<PlayerCommandSnapshot>());
        }

        public CommandQueueResult QueueChaos(GameState gameState, Guid playerId, Guid gangId, string sectorId,
            int turnNumber)
        {
            return new CommandQueueResult(CommandQueueRequestStatus.Success, "Chaos", GetQueue(gameState, playerId));
        }

        public CommandQueueResult QueueControl(GameState gameState, Guid playerId, Guid gangId, string sectorId,
            int turnNumber)
        {
            return new CommandQueueResult(CommandQueueRequestStatus.Success, "Control", GetQueue(gameState, playerId));
        }

        public CommandQueueResult QueueFabricate(GameState gameState, Guid playerId, Guid gangId, string itemName,
            int turnNumber)
        {
            return new CommandQueueResult(CommandQueueRequestStatus.Success, "Fabricate",
                GetQueue(gameState, playerId));
        }

        public CommandQueueResult QueueInfluence(GameState gameState, Guid playerId, Guid gangId, string sectorId,
            int turnNumber)
        {
            return new CommandQueueResult(CommandQueueRequestStatus.Success, "Influence",
                GetQueue(gameState, playerId));
        }

        public CommandQueueResult QueueMove(GameState gameState, Guid playerId, Guid gangId, string targetSectorId,
            int turnNumber)
        {
            return new CommandQueueResult(CommandQueueRequestStatus.Success, "Move", GetQueue(gameState, playerId));
        }

        public CommandQueueResult QueueResearch(GameState gameState, Guid playerId, Guid gangId, string projectId,
            int turnNumber)
        {
            return new CommandQueueResult(CommandQueueRequestStatus.Success, "Research", GetQueue(gameState, playerId));
        }

        public CommandQueueResult Remove(GameState gameState, Guid playerId, Guid gangId, int turnNumber)
        {
            return new CommandQueueResult(CommandQueueRequestStatus.Success, "Remove", GetQueue(gameState, playerId));
        }
    }

    private sealed class StubFinancePreviewService : IFinancePreviewService
    {
        public FinanceProjection BuildProjection(GameState gameState, Guid playerId)
        {
            var player = gameState.Game.GetPlayer(playerId);

            var cityCategories = new List<FinanceCategory>
            {
                new(FinanceCategoryType.Upkeep, "Upkeep", -5),
                new(FinanceCategoryType.SectorTax, "Sector Tax", 2),
                new(FinanceCategoryType.SiteProtection, "Site Protection", 3),
                new(FinanceCategoryType.ChaosEstimate, "Chaos (Estimate)", 1),
                new(FinanceCategoryType.CashAdjustment, "Cash Adjustment", 1)
            };

            var sectorCategories = new List<FinanceCategory>
            {
                new(FinanceCategoryType.Upkeep, "Upkeep", -2),
                new(FinanceCategoryType.SiteProtection, "Site Protection", 3),
                new(FinanceCategoryType.ChaosEstimate, "Chaos (Estimate)", 0)
            };

            var sectors = new List<FinanceSectorProjection>
            {
                new("Sector-1", "Sector 1", sectorCategories)
            };

            return new FinanceProjection(playerId, player.Name, cityCategories, sectors);
        }
    }

    private sealed class RecordingRecruitmentService : IRecruitmentService
    {
        private readonly Dictionary<Guid, List<RecruitmentOptionSnapshot>> _optionsByPlayer = new();

        public List<(Guid PlayerId, Guid OptionId, string SectorId, int TurnNumber)> HireCalls { get; } = new();
        public List<(Guid PlayerId, Guid OptionId, int TurnNumber)> DeclineCalls { get; } = new();

        public RecruitmentPoolSnapshot EnsurePool(GameState gameState, Guid playerId, int turnNumber)
        {
            if (!_optionsByPlayer.TryGetValue(playerId, out var options))
            {
                options = new List<RecruitmentOptionSnapshot>
                {
                    new(Guid.NewGuid(), 0, "Alpha", 100, 10, RecruitmentOptionState.Available),
                    new(Guid.NewGuid(), 1, "Beta", 120, 12, RecruitmentOptionState.Available),
                    new(Guid.NewGuid(), 2, "Gamma", 140, 14, RecruitmentOptionState.Available)
                };
                _optionsByPlayer[playerId] = options;
            }

            return BuildSnapshot(gameState, playerId, options);
        }

        public IReadOnlyList<RecruitmentRefreshResult> RefreshPools(GameState gameState, int turnNumber)
        {
            var results = new List<RecruitmentRefreshResult>();
            foreach (var player in gameState.PlayerOrder)
            {
                var snapshot = EnsurePool(gameState, player.Id, turnNumber);
                results.Add(new RecruitmentRefreshResult(snapshot, false));
            }

            return results;
        }

        public RecruitmentHireResult Hire(GameState gameState, Guid playerId, Guid optionId, string sectorId,
            int turnNumber)
        {
            HireCalls.Add((playerId, optionId, sectorId, turnNumber));
            var updated = UpdateOption(gameState, playerId, optionId, RecruitmentOptionState.Hired);
            var option = updated.Options.First(o => o.OptionId == optionId);
            var gangId = Guid.NewGuid();
            return new RecruitmentHireResult(RecruitmentActionStatus.Success, updated, option, gangId, sectorId, null);
        }

        public RecruitmentDeclineResult Decline(GameState gameState, Guid playerId, Guid optionId, int turnNumber)
        {
            DeclineCalls.Add((playerId, optionId, turnNumber));
            var updated = UpdateOption(gameState, playerId, optionId, RecruitmentOptionState.Declined);
            var option = updated.Options.First(o => o.OptionId == optionId);
            return new RecruitmentDeclineResult(RecruitmentActionStatus.Success, updated, option, null);
        }

        private RecruitmentPoolSnapshot UpdateOption(GameState gameState, Guid playerId, Guid optionId,
            RecruitmentOptionState newState)
        {
            if (!_optionsByPlayer.TryGetValue(playerId, out var options))
                throw new InvalidOperationException("Recruitment pool has not been initialised for this player.");

            var updated = options
                .Select(option => option.OptionId == optionId ? option with { State = newState } : option)
                .ToList();

            _optionsByPlayer[playerId] = updated;
            return BuildSnapshot(gameState, playerId, updated);
        }

        private static RecruitmentPoolSnapshot BuildSnapshot(GameState gameState, Guid playerId,
            IReadOnlyList<RecruitmentOptionSnapshot> options)
        {
            var player = gameState.Game.GetPlayer(playerId);
            var cloned = options.ToList();
            return new RecruitmentPoolSnapshot(playerId, player.Name, cloned);
        }
    }

    private sealed class RecordingEventWriter(ITurnEventLog log) : ITurnEventWriter
    {
        public List<TurnEvent> Events { get; } = new();

        public void Write(int turnNumber, TurnPhase phase, TurnEventType type, string description,
            CommandPhase? commandPhase = null)
        {
            var entry = new TurnEvent(turnNumber, phase, commandPhase, type, description, DateTimeOffset.UtcNow);
            Events.Add(entry);
            log.Append(entry);
        }

        public void WriteEconomy(int turnNumber, TurnPhase phase, PlayerEconomySnapshot snapshot)
        {
        }

        public void WriteAction(ActionResult result)
        {
            var entry = new TurnEvent(result.Context.TurnNumber, result.Context.Phase, result.Context.CommandPhase,
                TurnEventType.Action, result.ToString(), DateTimeOffset.UtcNow);
            Events.Add(entry);
            log.Append(entry);
        }
    }
}