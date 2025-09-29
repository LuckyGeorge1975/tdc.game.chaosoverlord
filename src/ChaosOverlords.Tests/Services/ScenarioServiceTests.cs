using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Tests.Services;

public sealed class ScenarioServiceTests
{
    [Fact]
    public async Task CreateNewGameAsync_BuildsInitialState()
    {
        var config = new ScenarioConfig
        {
            Type = ScenarioType.KillEmAll,
            Name = "Default",
            Seed = 12345,
            Players = new List<ScenarioPlayerConfig>
            {
                new()
                {
                    Name = "Player One",
                    Kind = PlayerKind.Human,
                    StartingCash = 250,
                    HeadquartersSectorId = "D4",
                    HeadquartersSiteName = "Arena",
                    StartingGangName = "Hackers"
                },
                new()
                {
                    Name = "CPU",
                    Kind = PlayerKind.AiEasy,
                    StartingCash = 150,
                    HeadquartersSectorId = "E5",
                    StartingGangName = "Bruisers"
                }
            }
        };

        var gangs = new List<GangData>
        {
            new()
            {
                Name = "Hackers",
                HiringCost = 10,
                UpkeepCost = 2,
                Combat = 3,
                Defense = 2,
                TechLevel = 4,
                Stealth = 5,
                Detect = 3,
                Chaos = 1,
                Control = 2,
                Heal = 0,
                Influence = 1,
                Research = 0,
                Strength = 3,
                BladeMelee = 1,
                Ranged = 0,
                Fighting = 0,
                MartialArts = 0
            },
            new()
            {
                Name = "Bruisers",
                HiringCost = 8,
                UpkeepCost = 3,
                Combat = 4,
                Defense = 3,
                TechLevel = 2,
                Stealth = 2,
                Detect = 2,
                Chaos = 1,
                Control = 1,
                Heal = 0,
                Influence = 0,
                Research = 0,
                Strength = 4,
                BladeMelee = 1,
                Ranged = 1,
                Fighting = 1,
                MartialArts = 0
            }
        };

        var sites = new List<SiteData>
        {
            new()
            {
                Name = "Arena",
                Resistance = 10,
                Support = 2,
                Tolerance = 3,
                Cash = 4,
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
            }
        };

        var rng = new StubRngService();
        var scenarioService = new ScenarioService(new StubDataService(gangs, sites), rng);

        var state = await scenarioService.CreateNewGameAsync(config, CancellationToken.None);

        Assert.Equal(config, state.Scenario);
        Assert.Equal(config.Seed, state.RandomSeed);
        Assert.Equal(config.Seed, rng.Seed);
        Assert.Equal(2, state.PlayerOrder.Count);
        Assert.IsType<Player>(state.PrimaryPlayer);
        Assert.Equal("Player One", state.PrimaryPlayer.Name);
        Assert.Equal(250, state.PrimaryPlayer.Cash);

        Assert.Equal(state.PrimaryPlayerId, state.CurrentPlayer.Id);
        Assert.Equal(2, state.Game.Players.Count);
        Assert.Equal(2, state.Game.Gangs.Count);

        var playerOneSector = state.Game.GetSector("D4");
        Assert.Equal(state.PrimaryPlayerId, playerOneSector.ControllingPlayerId);
        Assert.Single(playerOneSector.GangIds);

        var cpuPlayer = state.PlayerOrder[1];
        Assert.IsType<AiPlayer>(cpuPlayer);
        Assert.Equal(150, cpuPlayer.Cash);

        var cpuSector = state.Game.GetSector("E5");
        Assert.Equal(cpuPlayer.Id, cpuSector.ControllingPlayerId);
        Assert.Single(cpuSector.GangIds);

        var humanGang = state.Game.Gangs.Values.Single(g => g.OwnerId == state.PrimaryPlayerId);
        Assert.Equal("Hackers", humanGang.Data.Name);

        var cpuGang = state.Game.Gangs.Values.Single(g => g.OwnerId == cpuPlayer.Id);
        Assert.Equal("Bruisers", cpuGang.Data.Name);

        state.AdvanceToNextPlayer();
        Assert.Equal(cpuPlayer.Id, state.CurrentPlayer.Id);

        state.AdvanceTurn();
        Assert.Equal(state.PrimaryPlayerId, state.CurrentPlayer.Id);

        var manager = new GameStateManager(state);
        await manager.ExecuteCurrentPlayerAsync(CancellationToken.None);
        manager.AdvanceToNextPlayer();
        await manager.ExecuteCurrentPlayerAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CreateNewGameAsync_ThrowsWhenGangMissing()
    {
        var config = new ScenarioConfig
        {
            Type = ScenarioType.KillEmAll,
            Name = "Default",
            Players = new List<ScenarioPlayerConfig>
            {
                new()
                {
                    Name = "Player One",
                    Kind = PlayerKind.Human,
                    StartingCash = 250,
                    HeadquartersSectorId = "D4",
                    StartingGangName = "Unknown"
                }
            }
        };

        var gangs = new List<GangData>
        {
            new()
            {
                Name = "Hackers",
                HiringCost = 10,
                UpkeepCost = 2,
                Combat = 3,
                Defense = 2,
                TechLevel = 4,
                Stealth = 5,
                Detect = 3,
                Chaos = 1,
                Control = 2,
                Heal = 0,
                Influence = 1,
                Research = 0,
                Strength = 3,
                BladeMelee = 1,
                Ranged = 0,
                Fighting = 0,
                MartialArts = 0
            }
        };

        var scenarioService = new ScenarioService(new StubDataService(gangs, sites: Array.Empty<SiteData>()), new StubRngService());

        await Assert.ThrowsAsync<InvalidOperationException>(() => scenarioService.CreateNewGameAsync(config, CancellationToken.None));
    }

    [Fact]
    public async Task GameStateManager_ExecutesCurrentPlayer()
    {
        var playerOne = new TrackingPlayer(Guid.NewGuid(), "Player One");
        var playerTwo = new TrackingPlayer(Guid.NewGuid(), "Player Two");

        var game = new Game([playerOne, playerTwo], Array.Empty<Sector>());
        var state = new GameState(game, new ScenarioConfig
        {
            Type = ScenarioType.KillEmAll,
            Name = "Test",
            Players = new List<ScenarioPlayerConfig>
            {
                new() { Name = playerOne.Name, Kind = PlayerKind.Human, StartingGangName = "Hackers", HeadquartersSectorId = "A1" },
                new() { Name = playerTwo.Name, Kind = PlayerKind.Human, StartingGangName = "Hackers", HeadquartersSectorId = "B2" }
            }
        }, [playerOne, playerTwo], 0, randomSeed: 42);

        var manager = new GameStateManager(state);

        await manager.ExecuteCurrentPlayerAsync();
        Assert.Equal(1, playerOne.ExecutionCount);
        Assert.Equal(0, playerTwo.ExecutionCount);

        manager.AdvanceToNextPlayer();
        await manager.ExecuteCurrentPlayerAsync();
        Assert.Equal(1, playerTwo.ExecutionCount);
    }

    private sealed class StubDataService : IDataService
    {
        private readonly IReadOnlyList<GangData> _gangs;
        private readonly IReadOnlyList<ItemData> _items;
        private readonly IReadOnlyList<SiteData> _sites;
        private readonly IReadOnlyDictionary<int, ItemTypeData> _itemTypes;

        public StubDataService(
            IReadOnlyList<GangData> gangs,
            IReadOnlyList<SiteData> sites,
            IReadOnlyList<ItemData>? items = null,
            IReadOnlyDictionary<int, ItemTypeData>? itemTypes = null)
        {
            _gangs = gangs;
            _sites = sites;
            _items = items ?? Array.Empty<ItemData>();
            _itemTypes = itemTypes ?? new Dictionary<int, ItemTypeData>();
        }

        public Task<IReadOnlyList<GangData>> GetGangsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_gangs);

        public IReadOnlyList<GangData> GetGangs() => _gangs;

        public Task<IReadOnlyList<ItemData>> GetItemsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_items);

        public Task<IReadOnlyList<SiteData>> GetSitesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_sites);

        public Task<IReadOnlyDictionary<int, ItemTypeData>> GetItemTypesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_itemTypes);
    }

    private sealed class StubRngService : IRngService
    {
        public int Seed { get; private set; }

        public bool IsInitialised { get; private set; }

        public void Reset(int seed)
        {
            Seed = seed;
            IsInitialised = true;
        }

        public int NextInt() => Seed;

        public int NextInt(int minInclusive, int maxExclusive) => minInclusive;

        public double NextDouble() => 0.0;
    }

    private sealed class TrackingPlayer : PlayerBase
    {
        public TrackingPlayer(Guid id, string name)
            : base(id, name)
        {
        }

        public int ExecutionCount { get; private set; }

        public override Task ExecuteTurnAsync(GameStateManager manager, CancellationToken cancellationToken)
        {
            ExecutionCount++;
            return Task.CompletedTask;
        }
    }
}
