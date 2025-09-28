using ChaosOverlords.Data;

namespace ChaosOverlords.Tests.Data;

public class EmbeddedJsonDataServiceTests
{
    private readonly EmbeddedJsonDataService _sut = new();

    public static TheoryData<string, int, int, string, string> GangSamples => new()
    {
        { "Abominators", 6, 5, "Assets/Gangs/dummy_gang.svg", "Assets/Gangs/dummy_gang_thumbnail.svg" },
        { "Angels Of Arcadia", 11, 6, "Assets/Gangs/dummy_gang.svg", "Assets/Gangs/dummy_gang_thumbnail.svg" },
        { "Computer Hackers", 15, -1, "Assets/Gangs/dummy_gang.svg", "Assets/Gangs/dummy_gang_thumbnail.svg" }
    };

    public static TheoryData<string, int, int, int> ItemSamples => new()
    {
        { "Acid Blade", 1, 4, 4 },
        { "Assault Armor", 3, 4, 6 },
        { "Fusion Gun", 2, 12, 9 }
    };

    public static TheoryData<string, int, int> SiteSamples => new()
    {
        { "Research Lab", 12, 10 },
        { "Arena", 11, 0 },
        { "Factory", 10, 0 }
    };

    [Fact]
    public async Task GetGangsAsync_ProvidesCompleteDataset()
    {
        var gangs = await _sut.GetGangsAsync();

        Assert.Equal(90, gangs.Count);
        Assert.All(gangs, g => Assert.False(string.IsNullOrWhiteSpace(g.Name)));
    }

    [Theory]
    [MemberData(nameof(GangSamples))]
    public async Task GetGangsAsync_MapsExpectedSampleValues(string name, int hiringCost, int combat, string image, string thumbnail)
    {
        var gangs = await _sut.GetGangsAsync();
        var match = gangs.SingleOrDefault(g => g.Name == name);

        Assert.NotNull(match);

        var gang = match;

        Assert.Equal(hiringCost, gang.HiringCost);
        Assert.Equal(combat, gang.Combat);
        Assert.Equal(image, gang.Image);
        Assert.Equal(thumbnail, gang.Thumbnail);
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsCachedCollection()
    {
        var firstCall = await _sut.GetItemsAsync();
        var secondCall = await _sut.GetItemsAsync();

        Assert.Same(firstCall, secondCall);
        Assert.Equal(53, firstCall.Count);
    }

    [Theory]
    [MemberData(nameof(ItemSamples))]
    public async Task GetItemsAsync_MapsExpectedSampleValues(string name, int type, int researchCost, int techLevel)
    {
        var items = await _sut.GetItemsAsync();
        var match = items.SingleOrDefault(i => i.Name == name);

        Assert.NotNull(match);

        var item = match;

        Assert.Equal(type, item.Type);
        Assert.Equal(researchCost, item.ResearchCost);
        Assert.Equal(techLevel, item.TechLevel);
        Assert.False(string.IsNullOrWhiteSpace(item.Image));
        Assert.False(string.IsNullOrWhiteSpace(item.Thumbnail));
    }

    [Fact]
    public async Task GetSitesAsync_ProvidesCompleteDataset()
    {
        var sites = await _sut.GetSitesAsync();

        Assert.Equal(22, sites.Count);
    }

    [Theory]
    [MemberData(nameof(SiteSamples))]
    public async Task GetSitesAsync_MapsExpectedSampleValues(string name, int resistance, int researchThroughTechLevel)
    {
        var sites = await _sut.GetSitesAsync();
        var match = sites.SingleOrDefault(s => s.Name == name);

        Assert.NotNull(match);

        var site = match;

        Assert.Equal(resistance, site.Resistance);
        Assert.Equal(researchThroughTechLevel, site.EnablesResearchThroughTechLevel);
        Assert.False(string.IsNullOrWhiteSpace(site.Image));
        Assert.False(string.IsNullOrWhiteSpace(site.Thumbnail));
    }

    [Fact]
    public async Task GetItemTypesAsync_ReturnsExpectedMappings()
    {
        var itemTypes = await _sut.GetItemTypesAsync();

        Assert.Equal([0, 1, 2, 3, 4], itemTypes.Keys.OrderBy(k => k));

        Assert.Equal("Melee Weapon", itemTypes[0].Name);
        Assert.Equal("Blade Weapon", itemTypes[1].Name);
        Assert.Contains("Armor", itemTypes[3].Name);
    }
}
