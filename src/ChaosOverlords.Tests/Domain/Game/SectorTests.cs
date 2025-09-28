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

    private static Gang CreateGang()
    {
        return new Gang(Guid.NewGuid(), CreateGangData(), Guid.NewGuid(), "A1");
    }

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