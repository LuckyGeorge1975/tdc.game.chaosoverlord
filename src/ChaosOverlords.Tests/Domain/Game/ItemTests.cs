using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Tests.Domain.Game;

public sealed class ItemTests
{
    [Fact]
    public void Equip_SetsFlag()
    {
    var item = CreateItem();

        item.Equip();

        Assert.True(item.IsEquipped);
    }

    [Fact]
    public void Unequip_ClearsFlag()
    {
        var item = CreateItem(isEquipped: true);

        item.Unequip();

        Assert.False(item.IsEquipped);
    }

    private static Item CreateItem(bool isEquipped = false)
    {
        var data = new ItemData
        {
            Name = "Laser",
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

        return new Item(Guid.NewGuid(), data, isEquipped);
    }
}
