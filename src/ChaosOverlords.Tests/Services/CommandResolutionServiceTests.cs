using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Actions;
using ChaosOverlords.Core.Domain.Game.Commands;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Domain.Game.Events;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Tests.Services;

public sealed class CommandResolutionServiceTests
{
    [Fact]
    public void Execute_ResolvesCommandsAndClearsQueue()
    {
        var writer = new RecordingEventWriter();
        var rng = new DeterministicRngService();
        rng.Reset(123);
        var research = new ResearchService();
        var service = new CommandResolutionService(writer, rng, research);
        var context = CreateContext();
        var queue = context.State.Commands.GetOrCreate(context.PlayerId);

        var chaosCommand = new ChaosCommand(Guid.NewGuid(), context.PlayerId, context.ChaosGang.Id, 1, "B1", 5);
        var moveCommand = new MoveCommand(Guid.NewGuid(), context.PlayerId, context.MoveGang.Id, 1, "A1", "A2");
        var controlCommand = new ControlCommand(Guid.NewGuid(), context.PlayerId, context.ControlGang.Id, 1, "C1");

        queue.SetCommand(chaosCommand);
        queue.SetCommand(moveCommand);
        queue.SetCommand(controlCommand);

        var report = service.Execute(context.State, context.PlayerId, 1);

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
        var research = new ResearchService();
        var service = new CommandResolutionService(writer, rng, research);
        var context = CreateContext();

        var report = service.Execute(context.State, context.PlayerId, 1);

        Assert.Empty(report.Entries);
        Assert.Empty(writer.Events);
    }

    [Fact]
    public void ExecuteMove_Fails_WhenTargetSectorBecomesFull()
    {
        var writer = new RecordingEventWriter();
        var rng = new DeterministicRngService();
        rng.Reset(111);
        var research = new ResearchService();
        var service = new CommandResolutionService(writer, rng, research);
        var context = CreateContext();

        // Fill A2 with 6 gangs from the same owner before execution
        for (var i = 0; i < 6; i++)
        {
            var filler = new Gang(Guid.NewGuid(), new GangData { Name = $"Filler-{i}" }, context.PlayerId, "A2");
            context.State.Game.AddGang(filler);
        }

        var queue = context.State.Commands.GetOrCreate(context.PlayerId);
        var moveCommand = new MoveCommand(Guid.NewGuid(), context.PlayerId, context.MoveGang.Id, 1, "A1", "A2");
        queue.SetCommand(moveCommand);

        var report = service.Execute(context.State, context.PlayerId, 1);

        var entry = Assert.Single(report.Entries);
        Assert.Equal(CommandPhase.Movement, entry.Phase);
        Assert.Equal(CommandExecutionStatus.Failed, entry.Status);
        Assert.Equal("A1", context.MoveGang.SectorId);
    }

    [Fact]
    public void ExecuteInfluence_ReducesResistance_OnSuccess()
    {
        var writer = new RecordingEventWriter();
        var rng = new DeterministicRngService();
        rng.Reset(222);
        var research = new ResearchService();
        var service = new CommandResolutionService(writer, rng, research);
        var context = CreateContext();

        // Prepare a sector with resistance and control
        var sector = context.State.Game.GetSector("A1");
        sector.SetController(context.PlayerId);
        sector.ResetInfluence();
        Assert.True(sector.InfluenceResistance >= 0);

        // Ensure gang with some influence sits in A1
        var influenceGangData = new GangData { Name = "Influencers", Influence = 2 };
        var influenceGang = new Gang(Guid.NewGuid(), influenceGangData, context.PlayerId, "A1");
        context.State.Game.AddGang(influenceGang);

        var queue = context.State.Commands.GetOrCreate(context.PlayerId);
        var cmd = new InfluenceCommand(Guid.NewGuid(), context.PlayerId, influenceGang.Id, 1, "A1");
        queue.SetCommand(cmd);

        var before = sector.InfluenceResistance;
        var report = service.Execute(context.State, context.PlayerId, 1);

        var entry = Assert.Single(report.Entries);
        Assert.Equal(CommandPhase.Instant, entry.Phase);
        Assert.Equal(CommandExecutionStatus.Completed, entry.Status);
        Assert.Equal(before - 1, sector.InfluenceResistance);
        Assert.Contains(writer.Events, e => e.Type == TurnEventType.Action);
    }

    [Fact]
    public void ExecuteResearch_AddsProgressAndWritesEvent()
    {
        var writer = new RecordingEventWriter();
        var rng = new DeterministicRngService();
        rng.Reset(555);
        var research = new ResearchService();
        var service = new CommandResolutionService(writer, rng, research);
        var context = CreateContext();

        // Add a research-capable gang for player
        var rGang = new Gang(Guid.NewGuid(), new GangData { Name = "Scientist", Research = 4 }, context.PlayerId, "A2");
        context.State.Game.AddGang(rGang);

        var queue = context.State.Commands.GetOrCreate(context.PlayerId);
        var cmd = new ResearchCommand(Guid.NewGuid(), context.PlayerId, rGang.Id, 1, "Prototype-1");
        queue.SetCommand(cmd);

        var report = service.Execute(context.State, context.PlayerId, 1);

        var entry = Assert.Single(report.Entries);
        Assert.Equal(CommandPhase.Instant, entry.Phase);
        Assert.Equal(CommandExecutionStatus.Completed, entry.Status);
        Assert.Contains(writer.Events,
            e => e.CommandPhase == CommandPhase.Instant && e.Description.Contains("Research"));
        Assert.Equal(4, context.State.Research.GetOrCreate(context.PlayerId).Progress);
    }

    private static CommandContext CreateContext()
    {
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Commander", 200);

        var chaosGangData = new GangData { Name = "Chaos Crew", Chaos = 5 };
        var moveGangData = new GangData { Name = "Movers", Strength = 3 };
        var controlGangData = new GangData { Name = "Controllers", Control = 3, Strength = 4 };

        var chaosGang = new Gang(Guid.NewGuid(), chaosGangData, playerId, "B1");
        var moveGang = new Gang(Guid.NewGuid(), moveGangData, playerId, "A1");
        var controlGang = new Gang(Guid.NewGuid(), controlGangData, playerId, "C1");

        var sectors = new[]
        {
            new Sector("A1", CreateSite("A1 Block", resistance: 2), playerId),
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

        var state = new GameState(game, scenario, new List<IPlayer> { player }, 0, 99);
        return new CommandContext(state, playerId, chaosGang, moveGang, controlGang);
    }

    private static SiteData CreateSite(string name, int cash = 0, int tolerance = 0, int support = 0,
        int resistance = 0)
    {
        return new SiteData
        {
            Name = name,
            Cash = cash,
            Tolerance = tolerance,
            Support = support,
            Resistance = resistance
        };
    }

    private sealed record CommandContext(
        GameState State,
        Guid PlayerId,
        Gang ChaosGang,
        Gang MoveGang,
        Gang ControlGang);

    private sealed class RecordingEventWriter : ITurnEventWriter
    {
        public List<(int TurnNumber, TurnPhase Phase, CommandPhase? CommandPhase, TurnEventType Type, string Description
            )> Events { get; } = new();

        public void Write(int turnNumber, TurnPhase phase, TurnEventType type, string description,
            CommandPhase? commandPhase = null)
        {
            Events.Add((turnNumber, phase, commandPhase, type, description));
        }

        public void WriteEconomy(int turnNumber, TurnPhase phase, PlayerEconomySnapshot snapshot)
        {
        }

        public void WriteAction(ActionResult result)
        {
            Events.Add((result.Context.TurnNumber, result.Context.Phase, result.Context.CommandPhase,
                TurnEventType.Action, result.ToString()));
        }
    }
}