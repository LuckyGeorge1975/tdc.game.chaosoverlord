using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Tests.Domain.Game;

public sealed class SectorTests
{

    [Fact]
    public void PlaceGang_AddsGangId()
    {
        var sector = new Sector("A1", CreateSiteData());
        var gang = CreateGang();

        sector.PlaceGang(gang);

        Assert.Contains(gang.Id, sector.GangIds);
    }

    [Fact]
    public void RemoveGang_RemovesMatchingId()
    {
        var sector = new Sector("A1", CreateSiteData());
        var gang = CreateGang();
        sector.PlaceGang(gang);

        var removed = sector.RemoveGang(gang.Id);

        Assert.True(removed);
        Assert.Empty(sector.GangIds);
    }

    [Fact]
    public void SetController_UpdatesController()
    {
        var sector = new Sector("A1", CreateSiteData());
        var playerId = Guid.NewGuid();

        sector.SetController(playerId);

        Assert.Equal(playerId, sector.ControllingPlayerId);
    }

    [Fact]
    public void InfluenceResistance_InitializesFromSiteData()
    {
        var site = CreateSiteData(7);
        var sector = new Sector("A1", site);

        Assert.Equal(7, sector.InfluenceResistance);
        Assert.False(sector.IsInfluenced);
    }

    [Fact]
    public void ReduceInfluenceResistance_DecrementsAndClampsAtZero()
    {
        var site = CreateSiteData(3);
        var sector = new Sector("A1", site);

        var remaining = sector.ReduceInfluenceResistance(2);
        Assert.Equal(1, remaining);
        Assert.False(sector.IsInfluenced);

        remaining = sector.ReduceInfluenceResistance(5);
        Assert.Equal(0, remaining);
        Assert.True(sector.IsInfluenced);

        // Further reductions keep it at zero
        remaining = sector.ReduceInfluenceResistance(1);
        Assert.Equal(0, remaining);
    }

    [Fact]
    public void ResetInfluence_RestoresOriginalResistance()
    {
        var site = CreateSiteData(4);
        var sector = new Sector("A1", site);

        sector.ReduceInfluenceResistance(4);
        Assert.True(sector.IsInfluenced);

        sector.ResetInfluence();
        Assert.Equal(4, sector.InfluenceResistance);
        Assert.False(sector.IsInfluenced);
    }

    private static Gang CreateGang()
    {
        return new Gang(Guid.NewGuid(), CreateGangData(), Guid.NewGuid(), "A1");
    }

    private static SiteData CreateSiteData(int? resistance = null) => new()
    {
        Name = "Downtown Plaza",
        Resistance = resistance ?? 1,
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
}