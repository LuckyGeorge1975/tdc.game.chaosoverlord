using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Tests.Domain.Game;

public sealed class GameTests
{

    [Fact]
    public void AddGang_RegistersAcrossState()
    {
        var player = new Player(Guid.NewGuid(), "Player 1", 100);
    var sector = new Sector("A1", CreateSiteData());
        var game = new Core.Domain.Game.Game([player], [sector]);

        var gang = CreateGang(player.Id, sector.Id);

        game.AddGang(gang);

        var fetchedGang = game.GetGang(gang.Id);
        Assert.Same(gang, fetchedGang);
        Assert.Equal([gang.Id], player.GangIds);
        Assert.Contains(gang.Id, sector.GangIds);
    }

    [Fact]
    public void MoveGang_MovesBetweenSectors()
    {
        var player = new Player(Guid.NewGuid(), "Player 1", 100);
    var origin = new Sector("A1", CreateSiteData());
    var target = new Sector("B2", CreateSiteData());
        var game = new Core.Domain.Game.Game([player], [origin, target]);

        var gang = CreateGang(player.Id, origin.Id);
        game.AddGang(gang);

        game.MoveGang(gang.Id, target.Id);

        Assert.Equal(target.Id, gang.SectorId);
        Assert.DoesNotContain(gang.Id, origin.GangIds);
        Assert.Contains(gang.Id, target.GangIds);
    }

    [Fact]
    public void RegisterItem_AttachesItemToGang()
    {
        var player = new Player(Guid.NewGuid(), "Player 1", 100);
    var sector = new Sector("A1", CreateSiteData());
        var game = new Core.Domain.Game.Game([player], [sector]);

        var gang = CreateGang(player.Id, sector.Id);
        game.AddGang(gang);

        var itemData = CreateItemData("item_1");
        var item = new Item(Guid.NewGuid(), itemData);

        game.RegisterItem(item, gang.Id);

        Assert.True(game.Items.ContainsKey(item.Id));
        Assert.Contains(gang.Items, i => i.Id == item.Id);
        Assert.True(item.IsEquipped);
    }

    [Fact]
    public void RemoveItem_DetachesFromGang()
    {
        var player = new Player(Guid.NewGuid(), "Player 1", 100);
    var sector = new Sector("A1", CreateSiteData());
        var game = new Core.Domain.Game.Game([player], [sector]);

        var gang = CreateGang(player.Id, sector.Id);
        var item = new Item(Guid.NewGuid(), CreateItemData("item_1"));
        gang.AttachItem(item);

        game.AddGang(gang);

        var removed = game.RemoveItem(item.Id);

        Assert.True(removed);
        Assert.DoesNotContain(gang.Items, i => i.Id == item.Id);
        Assert.False(game.Items.ContainsKey(item.Id));
        Assert.False(item.IsEquipped);
    }

    private static Gang CreateGang(Guid ownerId, string sectorId)
    {
        return new Gang(Guid.NewGuid(), CreateGangData(), ownerId, sectorId);
    }

    private static GangData CreateGangData() => new()
    {
        Name = "Hackers",
        HiringCost = 5,
        UpkeepCost = 2,
        Combat = 3,
        Defense = 2,
        TechLevel = 4,
        Stealth = 5,
        Detect = 4,
        Chaos = 1,
        Control = 1,
        Heal = 0,
        Influence = 1,
        Research = 0,
        Strength = 3,
        BladeMelee = 1,
        Ranged = 0,
        Fighting = 0,
        MartialArts = 0
    };

    private static ItemData CreateItemData(string name) => new()
    {
        Name = name,
        Type = 2,
        ResearchCost = 2,
        FabricationCost = 4,
        TechLevel = 3,
        Combat = 1,
        Defense = 0,
        Stealth = 0,
        Detect = 0,
        Chaos = 0,
        Control = 0,
        Heal = 0,
        Influence = 0,
        Research = 0,
        Strength = 1,
        BladeMelee = 0,
        Ranged = 0,
        Fighting = 0,
        MartialArts = 0
    };

    private static SiteData CreateSiteData() => new()
    {
        Name = "Downtown Plaza",
        Resistance = 1,
        Support = 1,
        Tolerance = 1,
        Cash = 2,
        Combat = 0,
        Defense = 0,
        Stealth = 0,
        Detect = 0,
        Chaos = 0,
        Control = 0,
        Heal = 0,
        Influence = 0,
        Research = 0,
        Strength = 0,
        BladeMelee = 0,
        Ranged = 0,
        Fighting = 0,
        MartialArts = 0,
        EquipmentDiscountPercent = 0,
        EnablesResearchThroughTechLevel = 0,
        Security = 0
    };
}
