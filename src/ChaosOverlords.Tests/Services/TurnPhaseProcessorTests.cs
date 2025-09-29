using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Domain.Game.Events;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Tests.Services;

public sealed class TurnPhaseProcessorTests
{
    [Fact]
    public void StartTurn_TriggersUpkeepOncePerTurn()
    {
        var controller = new TurnController();
        var session = new StubGameSession();
        var economyService = new TrackingEconomyService();
        var eventWriter = new TrackingEventWriter();

        using var processor = new TurnPhaseProcessor(controller, session, economyService, eventWriter);

        controller.StartTurn();

        Assert.True(session.InitializeCalled);
        Assert.Equal(1, economyService.CallCount);
    Assert.NotEmpty(eventWriter.EconomyEvents);

        var guard = 32;
        while (controller.CanAdvancePhase && guard-- > 0)
        {
            controller.AdvancePhase();
        }

        if (controller.CanEndTurn)
        {
            controller.EndTurn();
        }
        controller.StartTurn();

        Assert.Equal(2, economyService.CallCount); // once per turn
    }

    private sealed class StubGameSession : IGameSession
    {
        private readonly GameState _state;
        private readonly GameStateManager _manager;

        public StubGameSession()
        {
            var playerOne = new Player(Guid.NewGuid(), "Player One", 100);
            var playerTwo = new Player(Guid.NewGuid(), "Player Two", 100);
            var sectorOne = new Sector("A1", controllingPlayerId: playerOne.Id);
            var sectorTwo = new Sector("B2", controllingPlayerId: playerTwo.Id);
            var game = new Game(new IPlayer[] { playerOne, playerTwo }, new[] { sectorOne, sectorTwo });
            var scenario = new ScenarioConfig
            {
                Type = ScenarioType.KillEmAll,
                Name = "Stub",
                Players = new List<ScenarioPlayerConfig>
                {
                    new()
                    {
                        Name = playerOne.Name,
                        Kind = PlayerKind.Human,
                        StartingCash = 100,
                        HeadquartersSectorId = "A1",
                        StartingGangName = "Hackers"
                    },
                    new()
                    {
                        Name = playerTwo.Name,
                        Kind = PlayerKind.Human,
                        StartingCash = 100,
                        HeadquartersSectorId = "B2",
                        StartingGangName = "Hackers"
                    }
                }
            };

            _state = new GameState(game, scenario, new List<IPlayer> { playerOne, playerTwo }, 0, randomSeed: 1);
            _manager = new GameStateManager(_state);
        }

        public bool IsInitialized { get; private set; }

        public ScenarioConfig Scenario => _state.Scenario;

        public GameState GameState => _state;

        public GameStateManager Manager => _manager;

        public bool InitializeCalled { get; private set; }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            InitializeCalled = true;
            IsInitialized = true;
            return Task.CompletedTask;
        }
    }

    private sealed class TrackingEconomyService : IEconomyService
    {
        public int CallCount { get; private set; }

        public EconomyReport ApplyUpkeep(GameState gameState, int turnNumber)
        {
            CallCount++;
            var players = gameState.Game.Players.Values
                .Select(player => new PlayerEconomySnapshot(
                    player.Id,
                    player.Name,
                    player.Cash,
                    0,
                    1,
                    0,
                    1,
                    player.Cash + 1))
                .ToList();

            return new EconomyReport(turnNumber, players);
        }
    }

    private sealed class TrackingEventWriter : ITurnEventWriter
    {
    public List<(int TurnNumber, PlayerEconomySnapshot Snapshot)> EconomyEvents { get; } = new();

        public void Write(int turnNumber, TurnPhase phase, TurnEventType type, string description, CommandPhase? commandPhase = null)
        {
        }

        public void WriteEconomy(int turnNumber, TurnPhase phase, PlayerEconomySnapshot snapshot)
        {
            EconomyEvents.Add((turnNumber, snapshot));
        }
    }

}
