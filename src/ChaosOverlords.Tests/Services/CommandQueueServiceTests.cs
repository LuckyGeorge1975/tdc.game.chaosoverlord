using System;
using System.Collections.Generic;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Commands;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;
using Xunit;

namespace ChaosOverlords.Tests.Services;

public sealed class CommandQueueServiceTests
{

    [Fact]
    public void QueueMove_AddsCommand_WhenInputIsValid()
    {
        var service = new CommandQueueService();
        var (state, playerId, gang) = CreateState();

        var result = service.QueueMove(state, playerId, gang.Id, targetSectorId: "A2", turnNumber: 1);

        Assert.Equal(CommandQueueRequestStatus.Success, result.Status);
        var snapshot = Assert.Single(result.Snapshot.Commands);
        Assert.Equal(gang.Id, snapshot.GangId);
        Assert.Equal(PlayerCommandKind.Move, snapshot.Kind);
        Assert.Equal("A1", snapshot.SourceSectorId);
        Assert.Equal("A2", snapshot.TargetSectorId);
    }

    [Fact]
    public void QueueMove_ReplacesExistingCommand_ForSameGang()
    {
        var service = new CommandQueueService();
        var (state, playerId, gang) = CreateState();

        var first = service.QueueMove(state, playerId, gang.Id, targetSectorId: "A2", turnNumber: 1);
        Assert.Equal(CommandQueueRequestStatus.Success, first.Status);

        var second = service.QueueMove(state, playerId, gang.Id, targetSectorId: "A2", turnNumber: 1);

        Assert.Equal(CommandQueueRequestStatus.Replaced, second.Status);
        Assert.NotNull(second.Command);
        Assert.NotNull(second.ReplacedCommand);
        Assert.Equal(second.Command!.CommandId, second.Snapshot.Commands.Single().CommandId);
    }

    [Fact]
    public void QueueMove_ReturnsNotAdjacent_WhenSectorsDoNotTouch()
    {
        var service = new CommandQueueService();
        var (state, playerId, gang) = CreateState();

        // From A1, A3 is not adjacent (two rows away)
        var result = service.QueueMove(state, playerId, gang.Id, targetSectorId: "A3", turnNumber: 1);

        Assert.Equal(CommandQueueRequestStatus.NotAdjacent, result.Status);
        Assert.Empty(result.Snapshot.Commands);
    }

    [Fact]
    public void QueueMove_Fails_WhenTargetSectorIsFullForOwner()
    {
        var service = new CommandQueueService();
        var (state, playerId, gang) = CreateState();

        // Fill target sector A2 with 6 of the same owner's gangs
        for (var i = 0; i < 6; i++)
        {
            var fillerGang = new Gang(Guid.NewGuid(), new GangData { Name = $"Filler-{i}" }, playerId, sectorId: "A2");
            state.Game.AddGang(fillerGang);
        }

        var result = service.QueueMove(state, playerId, gang.Id, targetSectorId: "A2", turnNumber: 1);

        Assert.Equal(CommandQueueRequestStatus.InvalidAction, result.Status);
        Assert.Empty(result.Snapshot.Commands);
    }

    [Fact]
    public void Remove_ReturnsRemoved_WhenCommandExists()
    {
        var service = new CommandQueueService();
        var (state, playerId, gang) = CreateState();
        service.QueueMove(state, playerId, gang.Id, targetSectorId: "A2", turnNumber: 1);

        var result = service.Remove(state, playerId, gang.Id, turnNumber: 1);

        Assert.Equal(CommandQueueRequestStatus.Removed, result.Status);
        Assert.NotNull(result.ReplacedCommand);
        Assert.Empty(result.Snapshot.Commands);
    }

    private static (GameState State, Guid PlayerId, Gang Gang) CreateState()
    {
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Player One");
        var sectors = new[]
        {
            new Sector("A1", CreateSite("A1 Block"), playerId),
            new Sector("A2", CreateSite("A2 Block")),
            new Sector("B2", CreateSite("B2 Block")),
            new Sector("A3", CreateSite("A3 Block"))
        };

        var gangData = new GangData
        {
            Name = "Grinders",
            Chaos = 3,
            Control = 2,
            Strength = 4
        };

        var gang = new Gang(Guid.NewGuid(), gangData, playerId, sectorId: "A1");

        var game = new Game(new IPlayer[] { player }, sectors, new[] { gang });
        var scenario = new ScenarioConfig
        {
            Type = ScenarioType.KillEmAll,
            Name = "Test",
            Players = new List<ScenarioPlayerConfig>
            {
                new()
                {
                    Name = player.Name,
                    Kind = PlayerKind.Human,
                    StartingCash = 0,
                    HeadquartersSectorId = "A1",
                    StartingGangName = gangData.Name
                }
            },
            MapSectorIds = sectors.Select(s => s.Id).ToList(),
            Seed = 42
        };

        var state = new GameState(game, scenario, new List<IPlayer> { player }, startingPlayerIndex: 0, randomSeed: 42);
        return (state, playerId, gang);
    }

    private static SiteData CreateSite(string name) => new()
    {
        Name = name,
        Cash = 0,
        Tolerance = 0
    };
}
