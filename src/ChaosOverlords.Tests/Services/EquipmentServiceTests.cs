using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Tests.Services;

public sealed class EquipmentServiceTests
{
    [Fact]
    public void Equip_Fails_If_Item_Not_In_Warehouse()
    {
        var (state, playerId, gang) = CreateState();
        var item = new Item(Guid.NewGuid(),
            new ItemData { Name = "Laser", TechLevel = 1, ResearchCost = 0, FabricationCost = 0 });
        var service = new EquipmentService();

        var result = service.Equip(state, playerId, gang.Id, item.Id, 1);

        Assert.Equal(EquipmentActionStatus.Failed, result.Status);
        Assert.Contains("warehouse", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Equip_Succeeds_When_TechLevel_OK_And_Item_Present()
    {
        var (state, playerId, gang) = CreateState(2);
        var item = new Item(Guid.NewGuid(),
            new ItemData { Name = "Laser", TechLevel = 1, ResearchCost = 0, FabricationCost = 0 });
        state.Warehouse.GetOrCreate(playerId).AddItem(item);
        var service = new EquipmentService();

        var result = service.Equip(state, playerId, gang.Id, item.Id, 1);

        Assert.Equal(EquipmentActionStatus.Success, result.Status);
        Assert.True(gang.ContainsItem(item.Id));
    }

    [Fact]
    public void Unequip_Returns_Item_To_Warehouse()
    {
        var (state, playerId, gang) = CreateState(3);
        var item = new Item(Guid.NewGuid(),
            new ItemData { Name = "Armor", TechLevel = 2, ResearchCost = 0, FabricationCost = 0 });
        state.Warehouse.GetOrCreate(playerId).AddItem(item);
        var service = new EquipmentService();

        var equip = service.Equip(state, playerId, gang.Id, item.Id, 1);
        Assert.Equal(EquipmentActionStatus.Success, equip.Status);

        var result = service.Unequip(state, playerId, gang.Id, item.Id, 1);
        Assert.Equal(EquipmentActionStatus.Success, result.Status);
        Assert.False(gang.ContainsItem(item.Id));
        Assert.True(state.Warehouse.GetOrCreate(playerId).TryGetItem(item.Id, out _));
    }

    [Fact]
    public void Give_Moves_Item_Between_Gangs_When_Tech_OK()
    {
        var (state, playerId, fromGang) = CreateState(3);
        var toGang = new Gang(Guid.NewGuid(), new GangData { Name = "Receivers", TechLevel = 3 }, playerId, "A1");
        state.Game.AddGang(toGang);
        var item = new Item(Guid.NewGuid(),
            new ItemData { Name = "Pistol", TechLevel = 2, ResearchCost = 0, FabricationCost = 10 });
        state.Warehouse.GetOrCreate(playerId).AddItem(item);
        var service = new EquipmentService();

        var equip = service.Equip(state, playerId, fromGang.Id, item.Id, 1);
        Assert.Equal(EquipmentActionStatus.Success, equip.Status);

        var give = service.Give(state, playerId, fromGang.Id, toGang.Id, item.Id, 1);
        Assert.Equal(EquipmentActionStatus.Success, give.Status);
        Assert.False(fromGang.ContainsItem(item.Id));
        Assert.True(toGang.ContainsItem(item.Id));
    }

    [Fact]
    public void Sell_Credits_Cash_And_Removes_Item()
    {
        var (state, playerId, gang) = CreateState(2);
        var player = (Player)state.Game.Players[playerId];
        var item = new Item(Guid.NewGuid(),
            new ItemData { Name = "SMG", TechLevel = 1, ResearchCost = 0, FabricationCost = 20 });
        state.Warehouse.GetOrCreate(playerId).AddItem(item);
        var service = new EquipmentService();

        var equip = service.Equip(state, playerId, gang.Id, item.Id, 1);
        Assert.Equal(EquipmentActionStatus.Success, equip.Status);

        var before = player.Cash;
        var sell = service.Sell(state, playerId, gang.Id, item.Id, 1);
        Assert.Equal(EquipmentActionStatus.Success, sell.Status);
        Assert.Equal(before + 10, player.Cash);
        Assert.False(gang.ContainsItem(item.Id));
    }

    private static (GameState State, Guid PlayerId, Gang Gang) CreateState(int techLevel = 1)
    {
        var player = new Player(Guid.NewGuid(), "Player One", 100);
        var gangData = new GangData { Name = "Techies", TechLevel = techLevel };
        var gang = new Gang(Guid.NewGuid(), gangData, player.Id, "A1");

        var game = new Game(new IPlayer[] { player },
            new[] { new Sector("A1", new SiteData { Name = "HQ" }, player.Id) }, new[] { gang });
        var scenario = new ScenarioConfig
        {
            Type = ScenarioType.KillEmAll,
            Name = "Equip Test",
            Players = new List<ScenarioPlayerConfig>
            {
                new()
                {
                    Name = player.Name,
                    Kind = PlayerKind.Human,
                    StartingCash = 100,
                    HeadquartersSectorId = "A1",
                    StartingGangName = gangData.Name
                }
            },
            MapSectorIds = new List<string> { "A1" },
            Seed = 1
        };

        var state = new GameState(game, scenario, new List<IPlayer> { player }, 0, 1);
        return (state, player.Id, gang);
    }
}