using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Tests.Services;

public sealed class Task18Tests
{
    [Fact]
    public void FinancePreview_Includes_Research_Category()
    {
        var (state, player, _) = CreateState(2, startingCash: 100);
        var service = new FinancePreviewService();
        var projection = service.BuildProjection(state, player.Id);

        Assert.Contains(projection.CityCategories, c => c.Type == FinanceCategoryType.Research);
        var research = projection.CityCategories.Single(c => c.Type == FinanceCategoryType.Research);
        Assert.Equal(0, research.Amount); // shown but not charging cash in Task 18
    }

    [Fact]
    public void Equip_Respects_Simple_Item_Capacity()
    {
        var (state, player, gang) = CreateState(gangTech: 5, startingCash: 100);

        // Two items OK, third should fail
        var i1 = new Item(Guid.NewGuid(), new ItemData { Name = "I1", TechLevel = 1 });
        var i2 = new Item(Guid.NewGuid(), new ItemData { Name = "I2", TechLevel = 1 });
        var i3 = new Item(Guid.NewGuid(), new ItemData { Name = "I3", TechLevel = 1 });
        state.Warehouse.GetOrCreate(player.Id).AddItem(i1);
        state.Warehouse.GetOrCreate(player.Id).AddItem(i2);
        state.Warehouse.GetOrCreate(player.Id).AddItem(i3);

        var svc = new EquipmentService();
        var r1 = svc.Equip(state, player.Id, gang.Id, i1.Id, 1);
        var r2 = svc.Equip(state, player.Id, gang.Id, i2.Id, 1);
        var r3 = svc.Equip(state, player.Id, gang.Id, i3.Id, 1);

        Assert.Equal(EquipmentActionStatus.Success, r1.Status);
        Assert.Equal(EquipmentActionStatus.Success, r2.Status);
        Assert.Equal(EquipmentActionStatus.Failed, r3.Status);
        Assert.Contains("capacity", r3.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static (GameState State, Player Player, Gang Gang) CreateState(int gangResearch = 0, int gangTech = 1,
        int startingCash = 50)
    {
        var player = new Player(Guid.NewGuid(), "P1", startingCash);
        var gangData = new GangData { Name = "G", Research = gangResearch, TechLevel = gangTech };
        var gang = new Gang(Guid.NewGuid(), gangData, player.Id, "A1");

        var sector = new Sector("A1", new SiteData { Name = "HQ" }, player.Id);
        var game = new Game(new IPlayer[] { player }, new[] { sector }, new[] { gang });
        var scenario = new ScenarioConfig
        {
            Type = ScenarioType.KillEmAll,
            Name = "Task18",
            Players = new List<ScenarioPlayerConfig>
            {
                new()
                {
                    Name = player.Name,
                    Kind = PlayerKind.Human,
                    StartingCash = startingCash,
                    HeadquartersSectorId = "A1",
                    StartingGangName = gangData.Name
                }
            },
            MapSectorIds = new List<string> { "A1" },
            Seed = 1
        };

        var state = new GameState(game, scenario, new List<IPlayer> { player }, 0, 1);
        return (state, player, gang);
    }
}