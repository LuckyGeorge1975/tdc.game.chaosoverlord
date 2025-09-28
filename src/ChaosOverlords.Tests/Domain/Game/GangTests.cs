using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Tests.Domain.Game;

public sealed class GangTests
{
    [Fact]
    public void MoveTo_UpdatesSector()
    {
        var gang = CreateGang();

        gang.MoveTo("B2");

        Assert.Equal("B2", gang.SectorId);
    }

    [Fact]
    public void AttachItem_AddsItemOnce()
    {
        var gang = CreateGang();
        var item = new Item(Guid.NewGuid(), CreateItemData());

        gang.AttachItem(item);
        gang.AttachItem(item);

        Assert.Single(gang.Items);
    }

    [Fact]
    public void DetachItem_RemovesMatchingItem()
    {
        var gang = CreateGang();
        var item = new Item(Guid.NewGuid(), CreateItemData());
        gang.AttachItem(item);

        var removed = gang.DetachItem(item.Id);

        Assert.True(removed);
        Assert.Empty(gang.Items);
    }

    [Fact]
    public void StrengthBreakdown_ReportsBaseLevelAndItemTotals()
    {
        var data = CreateGangData();
        var gang = new Gang(Guid.NewGuid(), data, Guid.NewGuid(), "A1");
        var item = new Item(Guid.NewGuid(), CreateItemData());
        gang.AttachItem(item);

        gang.ApplyLevelBonus(new StatSheet(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0));

        var strength = gang.StrengthBreakdown;

        Assert.Equal(data.Strength, strength.Base);
        Assert.Equal(2, strength.Level);
        Assert.Equal(item.Modifiers.Strength, strength.Items);
        Assert.Equal(strength.Base + strength.Level + strength.Items, gang.Strength);
    }

    private static Gang CreateGang()
    {
        return new Gang(Guid.NewGuid(), CreateGangData(), Guid.NewGuid(), "A1");
    }

    private static GangData CreateGangData()
    {
        return new GangData
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
    }

    private static ItemData CreateItemData() => new()
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
}
