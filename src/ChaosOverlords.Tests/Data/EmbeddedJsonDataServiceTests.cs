using System.Linq;
using ChaosOverlords.Data;

namespace ChaosOverlords.Tests.Data;

public class EmbeddedJsonDataServiceTests
{
    private readonly EmbeddedJsonDataService _sut = new();

    [Fact]
    public async Task GetGangsAsync_ReturnsExpectedGang()
    {
        var gangs = await _sut.GetGangsAsync();

        Assert.NotEmpty(gangs);
        var abominators = gangs.FirstOrDefault(g => g.Name == "Abominators");
        Assert.NotNull(abominators);
        Assert.Equal(6, abominators!.HiringCost);
        Assert.Equal("Assets/Gangs/dummy_gang.svg", abominators.Image);
        Assert.Equal("Assets/Gangs/dummy_gang_thumbnail.svg", abominators.Thumbnail);
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsCachedCollectionWithMetadata()
    {
        var firstCall = await _sut.GetItemsAsync();
        var secondCall = await _sut.GetItemsAsync();

        Assert.Same(firstCall, secondCall);
        Assert.NotEmpty(firstCall);

        var acidBlade = firstCall.FirstOrDefault(i => i.Name == "Acid Blade");
        Assert.NotNull(acidBlade);
        Assert.Equal(1, acidBlade!.Type);
        Assert.Equal("Assets/Items/dummy_item.svg", acidBlade.Image);
        Assert.Equal("Assets/Items/dummy_item_thumbnail.svg", acidBlade.Thumbnail);
    }

    [Fact]
    public async Task GetSitesAsync_ReturnsResearchLabWithExpectedValues()
    {
        var sites = await _sut.GetSitesAsync();

        Assert.NotEmpty(sites);
        var researchLab = sites.FirstOrDefault(s => s.Name == "Research Lab");
        Assert.NotNull(researchLab);
        Assert.Equal(12, researchLab!.Resistance);
        Assert.Equal(10, researchLab.EnablesResearchThroughTechLevel);
    }

    [Fact]
    public async Task GetItemTypesAsync_ContainsBladeEntry()
    {
        var itemTypes = await _sut.GetItemTypesAsync();

        Assert.NotEmpty(itemTypes);
        Assert.True(itemTypes.ContainsKey(0));

        var bladeType = itemTypes[0];
        Assert.False(string.IsNullOrWhiteSpace(bladeType.Name));
    }
}
