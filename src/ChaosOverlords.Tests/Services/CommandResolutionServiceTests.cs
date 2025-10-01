using System;
using System.Collections.Generic;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Actions;
using ChaosOverlords.Core.Domain.Game.Commands;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Domain.Game.Events;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;
using Xunit;

namespace ChaosOverlords.Tests.Services;

public sealed class CommandResolutionServiceTests
{

    [Fact]
    public void Execute_ResolvesCommandsAndClearsQueue()
    {
    var writer = new RecordingEventWriter();
    var rng = new DeterministicRngService();
    rng.Reset(123);
    var service = new CommandResolutionService(writer, rng);
        var context = CreateContext();
        var queue = context.State.Commands.GetOrCreate(context.PlayerId);

    var chaosCommand = new ChaosCommand(Guid.NewGuid(), context.PlayerId, context.ChaosGang.Id, TurnNumber: 1, SectorId: "B1", ProjectedChaos: 5);
    var moveCommand = new MoveCommand(Guid.NewGuid(), context.PlayerId, context.MoveGang.Id, TurnNumber: 1, SourceSectorId: "A1", TargetSectorId: "A2");
    var controlCommand = new ControlCommand(Guid.NewGuid(), context.PlayerId, context.ControlGang.Id, TurnNumber: 1, SectorId: "C1");

        queue.SetCommand(chaosCommand);
        queue.SetCommand(moveCommand);
        queue.SetCommand(controlCommand);

        var report = service.Execute(context.State, context.PlayerId, turnNumber: 1);

        Assert.Equal(context.PlayerId, report.PlayerId);
        Assert.Equal(3, report.Entries.Count);
        Assert.Collection(
            report.Entries,
            entry => Assert.Equal(CommandPhase.Chaos, entry.Phase),
            entry => Assert.Equal(CommandPhase.Movement, entry.Phase),
            entry => Assert.Equal(CommandPhase.Control, entry.Phase));

        Assert.All(report.Entries, entry => Assert.Equal(CommandExecutionStatus.Completed, entry.Status));

        Assert.Equal("A2", context.MoveGang.SectorId);
        Assert.Equal(context.PlayerId, context.State.Game.GetSector("C1").ControllingPlayerId);
        Assert.Equal(5, context.State.Game.GetSector("B1").ProjectedChaos);
        Assert.Equal(3, writer.Events.Count);
        Assert.Empty(queue.Commands);
        Assert.False(context.State.Commands.TryGet(context.PlayerId, out _));
    }

    [Fact]
    public void Execute_ReturnsEmptyReport_WhenQueueMissing()
    {
    var writer = new RecordingEventWriter();
    var rng = new DeterministicRngService();
    rng.Reset(321);
    var service = new CommandResolutionService(writer, rng);
        var context = CreateContext();

        var report = service.Execute(context.State, context.PlayerId, turnNumber: 1);

        Assert.Empty(report.Entries);
        Assert.Empty(writer.Events);
    }

    private static CommandContext CreateContext()
    {
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Commander", cash: 200);

        var chaosGangData = new GangData { Name = "Chaos Crew", Chaos = 5 };
        var moveGangData = new GangData { Name = "Movers", Strength = 3 };
        var controlGangData = new GangData { Name = "Controllers", Control = 3, Strength = 4 };

        var chaosGang = new Gang(Guid.NewGuid(), chaosGangData, playerId, sectorId: "B1");
        var moveGang = new Gang(Guid.NewGuid(), moveGangData, playerId, sectorId: "A1");
        var controlGang = new Gang(Guid.NewGuid(), controlGangData, playerId, sectorId: "C1");

        var sectors = new[]
        {
            new Sector("A1", CreateSite("A1 Block"), playerId),
            new Sector("A2", CreateSite("A2 Block")),
            new Sector("B1", CreateSite("B1 Block")),
            new Sector("C1", new SiteData { Name = "Mall", Cash = 0, Support = 0, Tolerance = 0 })
        };

        var game = new Game(new IPlayer[] { player }, sectors, new[] { chaosGang, moveGang, controlGang });
        var scenario = new ScenarioConfig
        {
            Type = ScenarioType.KillEmAll,
            Name = "Command Test",
            Players = new List<ScenarioPlayerConfig>
            {
                new()
                {
                    Name = player.Name,
                    Kind = PlayerKind.Human,
                    StartingCash = 200,
                    HeadquartersSectorId = "A1",
                    StartingGangName = chaosGangData.Name
                }
            },
            MapSectorIds = sectors.Select(s => s.Id).ToList(),
            Seed = 99
        };

        var state = new GameState(game, scenario, new List<IPlayer> { player }, startingPlayerIndex: 0, randomSeed: 99);
        return new CommandContext(state, playerId, chaosGang, moveGang, controlGang);
    }

    private sealed record CommandContext(GameState State, Guid PlayerId, Gang ChaosGang, Gang MoveGang, Gang ControlGang);

    private static SiteData CreateSite(string name, int cash = 0, int tolerance = 0, int support = 0) => new()
    {
        Name = name,
        Cash = cash,
        Tolerance = tolerance,
        Support = support
    };

    private sealed class RecordingEventWriter : ITurnEventWriter
    {
        public List<(int TurnNumber, TurnPhase Phase, CommandPhase? CommandPhase, TurnEventType Type, string Description)> Events { get; } = new();

        public void Write(int turnNumber, TurnPhase phase, TurnEventType type, string description, CommandPhase? commandPhase = null)
        {
            Events.Add((turnNumber, phase, commandPhase, type, description));
        }

        public void WriteEconomy(int turnNumber, TurnPhase phase, PlayerEconomySnapshot snapshot)
        {
        }

        public void WriteAction(ActionResult result)
        {
            Events.Add((result.Context.TurnNumber, result.Context.Phase, result.Context.CommandPhase, TurnEventType.Action, result.ToString()));
        }
    }
}
