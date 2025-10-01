using System;
using System.Collections.Generic;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Tests.Services;

public sealed class EconomyServiceTests
{

    [Fact]
    public void ApplyUpkeep_AdjustsPlayerCashAndProducesSnapshot()
    {
        var (state, turnNumber) = CreateState();
        var playerOne = (Player)state.Game.GetPlayer(state.PrimaryPlayerId);
        var playerTwo = (Player)state.Game.Players.Values.Single(p => p.Id != state.PrimaryPlayerId);

        var service = new EconomyService();

        var report = service.ApplyUpkeep(state, turnNumber);

        var playerOneSnapshot = report.PlayerSnapshots.Single(p => p.PlayerId == playerOne.Id);
        var playerTwoSnapshot = report.PlayerSnapshots.Single(p => p.PlayerId == playerTwo.Id);

        Assert.Equal(1, report.TurnNumber);

    // Player one: upkeep 5, sector income 5 => net 0 (200 -> 200)
        Assert.Equal(5, playerOneSnapshot.Upkeep);
    Assert.Equal(5, playerOneSnapshot.SectorIncome);
    Assert.Equal(0, playerOneSnapshot.SiteIncome);
        Assert.Equal(0, playerOneSnapshot.NetChange);
        Assert.Equal(200, playerOneSnapshot.EndingCash);

    // Player two: upkeep 3, sector income -1 => net -4 (150 -> 146)
        Assert.Equal(3, playerTwoSnapshot.Upkeep);
    Assert.Equal(-1, playerTwoSnapshot.SectorIncome);
    Assert.Equal(0, playerTwoSnapshot.SiteIncome);
        Assert.Equal(-4, playerTwoSnapshot.NetChange);
        Assert.Equal(146, playerTwoSnapshot.EndingCash);

        Assert.Equal(200, playerOne.Cash);
        Assert.Equal(146, playerTwo.Cash);
        Assert.Equal(8, report.TotalUpkeep);
    Assert.Equal(4, report.TotalSectorIncome);
    Assert.Equal(0, report.TotalSiteIncome);
        Assert.Equal(-4, report.TotalNetChange); // matches sum of nets
    }

    [Fact]
    public void ApplyUpkeep_WithNoControlledSectors_HandlesZeroIncome()
    {
        var (state, turnNumber) = CreateState(includeNeutralSectors: false);
        var player = (Player)state.Game.GetPlayer(state.PrimaryPlayerId);

        foreach (var sector in state.Game.Sectors.Values)
        {
            sector.SetController(null);
        }

        var service = new EconomyService();
        var report = service.ApplyUpkeep(state, turnNumber);

        var snapshot = Assert.Single(report.PlayerSnapshots, p => p.PlayerId == player.Id);
        Assert.Equal(0, snapshot.SectorIncome);
        Assert.Equal(0, snapshot.SiteIncome);
        Assert.Equal(-5, snapshot.NetChange);
        Assert.Equal(player.Cash, snapshot.EndingCash);
    }

    private static (GameState State, int TurnNumber) CreateState(bool includeNeutralSectors = true)
    {
        var playerOne = new Player(Guid.NewGuid(), "Player One", 200);
        var playerTwo = new Player(Guid.NewGuid(), "Player Two", 150);

        var sectors = new List<Sector>
        {
            new("A1", CreateSite("Arena", cash: 3, tolerance: 1, support: 1), playerOne.Id),
            new("B2", CreateSite("Warehouse", cash: 2), playerOne.Id),
            new("C3", CreateSite("Night Market", cash: -1, tolerance: 1, support: 1), playerTwo.Id)
        };

        if (includeNeutralSectors)
        {
            sectors.Add(new Sector("D4", CreateSite("Old Docks")));
        }

        var gangs = new List<Gang>
        {
            new(Guid.NewGuid(), new GangData
            {
                Name = "Hackers",
                HiringCost = 10,
                UpkeepCost = 2,
                Combat = 2,
                Defense = 1,
                TechLevel = 4,
                Stealth = 5,
                Detect = 2,
                Chaos = 1,
                Control = 2,
                Heal = 0,
                Influence = 1,
                Research = 0,
                Strength = 3,
                BladeMelee = 1,
                Ranged = 0,
                Fighting = 0,
                MartialArts = 0
            }, playerOne.Id, "A1"),
            new(Guid.NewGuid(), new GangData
            {
                Name = "Bruisers",
                HiringCost = 8,
                UpkeepCost = 3,
                Combat = 4,
                Defense = 3,
                TechLevel = 2,
                Stealth = 2,
                Detect = 2,
                Chaos = 1,
                Control = 1,
                Heal = 0,
                Influence = 0,
                Research = 0,
                Strength = 4,
                BladeMelee = 1,
                Ranged = 1,
                Fighting = 1,
                MartialArts = 0
            }, playerTwo.Id, "C3"),
            new(Guid.NewGuid(), new GangData
            {
                Name = "Riggers",
                HiringCost = 6,
                UpkeepCost = 3,
                Combat = 3,
                Defense = 2,
                TechLevel = 3,
                Stealth = 1,
                Detect = 1,
                Chaos = 1,
                Control = 1,
                Heal = 0,
                Influence = 0,
                Research = 0,
                Strength = 3,
                BladeMelee = 0,
                Ranged = 0,
                Fighting = 0,
                MartialArts = 0
            }, playerOne.Id, "B2")
        };

        var game = new Game(new[] { playerOne, playerTwo }, sectors, gangs);

        var scenario = new ScenarioConfig
        {
            Type = ScenarioType.KillEmAll,
            Name = "Test Scenario",
            Players = new List<ScenarioPlayerConfig>
            {
                new()
                {
                    Name = playerOne.Name,
                    Kind = PlayerKind.Human,
                    StartingCash = 200,
                    HeadquartersSectorId = "A1",
                    StartingGangName = "Hackers"
                },
                new()
                {
                    Name = playerTwo.Name,
                    Kind = PlayerKind.Human,
                    StartingCash = 150,
                    HeadquartersSectorId = "C3",
                    StartingGangName = "Bruisers"
                }
            }
        };

        var state = new GameState(game, scenario, new List<IPlayer> { playerOne, playerTwo }, 0, randomSeed: 1);
        return (state, 1);
    }

    private static SiteData CreateSite(string name, int cash = 0, int tolerance = 0, int support = 0) => new()
    {
        Name = name,
        Cash = cash,
        Tolerance = tolerance,
        Support = support
    };
}
