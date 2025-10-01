using System;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;
using Xunit;

namespace ChaosOverlords.Tests.Services;

public sealed class FinancePreviewServiceTests
{

    [Fact]
    public void BuildProjection_ComputesCityAndSectorBreakdown()
    {
        var (state, primaryPlayer, _) = CreateStateWithAssets();
        var service = new FinancePreviewService();

        var projection = service.BuildProjection(state, primaryPlayer.Id);

        Assert.Equal(primaryPlayer.Id, projection.PlayerId);
        Assert.Equal(primaryPlayer.Name, projection.PlayerName);

        Assert.Equal(8, projection.CityCategories.Count);

        Assert.Contains(projection.CityCategories, category => category.Type == FinanceCategoryType.Upkeep && category.Amount == -5);
        Assert.Contains(projection.CityCategories, category => category.Type == FinanceCategoryType.SectorTax && category.Amount == 3);
        Assert.Contains(projection.CityCategories, category => category.Type == FinanceCategoryType.SiteProtection && category.Amount == 0);
        Assert.Contains(projection.CityCategories, category => category.Type == FinanceCategoryType.ChaosEstimate && category.Amount == 3);

        var netCategory = Assert.Single(projection.CityCategories, category => category.Type == FinanceCategoryType.CashAdjustment);
        Assert.Equal(1, netCategory.Amount);
        Assert.Equal(1, projection.NetCashAdjustment);

        Assert.Equal(2, projection.Sectors.Count);

        var sectorA1 = projection.Sectors.Single(sector => sector.SectorId == "A1");
        Assert.Equal("A1 – Neon Hub", sectorA1.DisplayName);
        Assert.Equal(3, sectorA1.NetChange);
        Assert.Contains(sectorA1.Categories, category => category.Type == FinanceCategoryType.Upkeep && category.Amount == -3);
        Assert.Contains(sectorA1.Categories, category => category.Type == FinanceCategoryType.SectorTax && category.Amount == 4);
        Assert.Contains(sectorA1.Categories, category => category.Type == FinanceCategoryType.SiteProtection && category.Amount == 0);
        Assert.Contains(sectorA1.Categories, category => category.Type == FinanceCategoryType.ChaosEstimate && category.Amount == 2);

        var sectorB2 = projection.Sectors.Single(sector => sector.SectorId == "B2");
        Assert.Equal(-2, sectorB2.NetChange);
        Assert.Contains(sectorB2.Categories, category => category.Type == FinanceCategoryType.Upkeep && category.Amount == -2);
        Assert.Contains(sectorB2.Categories, category => category.Type == FinanceCategoryType.SectorTax && category.Amount == -1);
        Assert.Contains(sectorB2.Categories, category => category.Type == FinanceCategoryType.SiteProtection && category.Amount == 0);
    }

    [Fact]
    public void BuildProjection_WithNoControlledSectors_ReturnsEmptyCollections()
    {
        var (state, primaryPlayer, enemyPlayer) = CreateStateWithAssets();

        foreach (var sector in state.Game.Sectors.Values)
        {
            sector.SetController(enemyPlayer.Id);
            sector.ClearChaosProjection();
        }

        foreach (var gang in state.Game.Gangs.Values.ToList())
        {
            if (gang.OwnerId == primaryPlayer.Id)
            {
                state.Game.RemoveGang(gang.Id);
            }
        }

        var service = new FinancePreviewService();
        var projection = service.BuildProjection(state, primaryPlayer.Id);

        Assert.All(projection.CityCategories.Where(category => category.Type != FinanceCategoryType.CashAdjustment), category => Assert.Equal(0, category.Amount));
        Assert.Equal(0, projection.NetCashAdjustment);
        Assert.Empty(projection.Sectors);
    }

    private static (GameState State, Player Primary, Player Enemy) CreateStateWithAssets()
    {
        var primaryPlayer = new Player(Guid.NewGuid(), "Player One", 120);
        var enemyPlayer = new Player(Guid.NewGuid(), "Player Two", 90);

        var sectorA1 = new Sector("A1", new SiteData { Name = "Neon Hub", Cash = 4, Tolerance = 2 }, primaryPlayer.Id);
        var sectorB2 = new Sector("B2", new SiteData { Name = "Midnight Exchange", Cash = -1, Tolerance = 1 }, primaryPlayer.Id);
        var sectorC3 = new Sector("C3", new SiteData { Name = "Shadow Market", Cash = 0, Tolerance = 1 }, enemyPlayer.Id);

        var gangOne = new Gang(Guid.NewGuid(), new GangData { Name = "Ghosts", HiringCost = 10, UpkeepCost = 3 }, primaryPlayer.Id, sectorA1.Id);
        var gangTwo = new Gang(Guid.NewGuid(), new GangData { Name = "Rogues", HiringCost = 8, UpkeepCost = 2 }, primaryPlayer.Id, sectorB2.Id);
        var enemyGang = new Gang(Guid.NewGuid(), new GangData { Name = "Enforcers", HiringCost = 7, UpkeepCost = 4 }, enemyPlayer.Id, sectorC3.Id);

        var game = new Game(new[] { primaryPlayer, enemyPlayer }, new[] { sectorA1, sectorB2, sectorC3 }, new[] { gangOne, gangTwo, enemyGang });

        game.GetSector("A1").SetChaosProjection(2);
        game.GetSector("B2").SetChaosProjection(1);
        game.GetSector("C3").SetChaosProjection(0);

        var scenario = new ScenarioConfig
        {
            Type = ScenarioType.KillEmAll,
            Name = "Test Scenario",
            Players = new[]
            {
                new ScenarioPlayerConfig
                {
                    Name = primaryPlayer.Name,
                    Kind = PlayerKind.Human,
                    StartingCash = primaryPlayer.Cash,
                    HeadquartersSectorId = sectorA1.Id,
                    StartingGangName = gangOne.Data.Name
                },
                new ScenarioPlayerConfig
                {
                    Name = enemyPlayer.Name,
                    Kind = PlayerKind.AiEasy,
                    StartingCash = enemyPlayer.Cash,
                    HeadquartersSectorId = sectorC3.Id,
                    StartingGangName = enemyGang.Data.Name
                }
            },
            MapSectorIds = new[] { sectorA1.Id, sectorB2.Id, sectorC3.Id },
            Seed = 1234
        };

        var state = new GameState(game, scenario, new IPlayer[] { primaryPlayer, enemyPlayer });
        return (state, primaryPlayer, enemyPlayer);
    }
}
