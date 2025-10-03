using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Tests.Services;

public sealed class ResearchServiceTests
{
    [Fact]
    public void BuildPreview_ReturnsSumOfPlayerResearch()
    {
        var (state, playerId) = CreateMinimalState();
        var service = new ResearchService();

        var preview = service.BuildPreview(state, playerId);

        // Two gangs with research 2 and 3
        Assert.Equal(5, preview.EstimatedProgress);
        Assert.Null(preview.ActiveProjectId);
        Assert.Equal(0, preview.EstimatedCost);
    }

    [Fact]
    public void Execute_SelectsProjectAndAddsProgress()
    {
        var (state, playerId) = CreateMinimalState();
        var service = new ResearchService();

        var result = service.Execute(state, playerId, "Proj-X", 1);

        Assert.Equal(ResearchActionStatus.Success, result.Status);
        Assert.Equal("Proj-X", result.ProjectId);
        Assert.Equal(5, result.ProgressApplied); // two gangs: 2 + 3

        var playerResearch = state.Research.GetOrCreate(playerId);
        Assert.Equal("Proj-X", playerResearch.ActiveProjectId);
        Assert.Equal(5, playerResearch.Progress);

        // Next turn contribute again should accumulate
        var result2 = service.Execute(state, playerId, "Proj-X", 2);
        Assert.Equal(5, result2.ProgressApplied);
        Assert.Equal(10, state.Research.GetOrCreate(playerId).Progress);
    }

    [Fact]
    public void Execute_ValidatesProjectAgainstItems_AndReportsCompletion()
    {
        var (state, playerId) = CreateMinimalState();
        var items = new List<ItemData>
        {
            new() { Name = "Fusion Gun", ResearchCost = 5, TechLevel = 9 }
        };
        var data = new StubDataService(items);
        var service = new ResearchService(data);

        var result = service.Execute(state, playerId, "Fusion Gun", 1);
        Assert.Equal(ResearchActionStatus.Success, result.Status);
        Assert.Contains(state.Research.GetOrCreate(playerId).Progress.ToString(), result.Message);

        // Second call should mark completed
        var result2 = service.Execute(state, playerId, "Fusion Gun", 2);
        Assert.Contains("Completed!", result2.Message);
    }

    private static (GameState State, Guid PlayerId) CreateMinimalState()
    {
        var playerId = Guid.NewGuid();
        var player = new Player(playerId, "Commander");

        var g1 = new Gang(Guid.NewGuid(), new GangData { Name = "R1", Research = 2 }, playerId, "A1");
        var g2 = new Gang(Guid.NewGuid(), new GangData { Name = "R2", Research = 3 }, playerId, "A2");

        var sectors = new[]
        {
            new Sector("A1", new SiteData { Name = "S1" }),
            new Sector("A2", new SiteData { Name = "S2" })
        };

        var game = new Game(new IPlayer[] { player }, sectors, new[] { g1, g2 });
        var scenario = new ScenarioConfig
        {
            Type = ScenarioType.KillEmAll,
            Name = "Research Test",
            Players = new List<ScenarioPlayerConfig>
            {
                new()
                {
                    Name = player.Name,
                    Kind = PlayerKind.Human,
                    StartingCash = 100,
                    HeadquartersSectorId = "A1",
                    StartingGangName = "R1"
                }
            },
            MapSectorIds = sectors.Select(s => s.Id).ToList(),
            Seed = 42
        };

        var state = new GameState(game, scenario, new List<IPlayer> { player }, 0, 42);
        return (state, playerId);
    }

    private sealed class StubDataService(IReadOnlyList<ItemData> items) : IDataService
    {
        public Task<IReadOnlyList<GangData>> GetGangsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<GangData>>(Array.Empty<GangData>());
        }

        public IReadOnlyList<GangData> GetGangs()
        {
            return Array.Empty<GangData>();
        }

        public Task<IReadOnlyList<ItemData>> GetItemsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(items);
        }

        public Task<IReadOnlyList<SiteData>> GetSitesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<SiteData>>(Array.Empty<SiteData>());
        }

        public Task<IReadOnlyDictionary<int, ItemTypeData>> GetItemTypesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyDictionary<int, ItemTypeData>>(new Dictionary<int, ItemTypeData>());
        }

        public Task<SectorConfigurationData> GetSectorConfigurationAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SectorConfigurationData());
        }
    }
}