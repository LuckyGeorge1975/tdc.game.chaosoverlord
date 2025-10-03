using System.Collections.ObjectModel;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Actions;
using ChaosOverlords.Core.Domain.Game.Recruitment;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Tests.Services;

public sealed class RecruitmentServiceTests
{
    [Fact]
    public void EnsurePool_CreatesThreeDistinctOptions()
    {
        var gangs = CreateGangData();
        var dataService = new TestDataService(gangs);
        var rng = new SequenceRngService(new[] { 0, 0, 0 });
        var service = new RecruitmentService(dataService, rng);
        var state = CreateGameState();

        var snapshot = service.EnsurePool(state, state.PrimaryPlayerId, 1);

        Assert.Equal(3, snapshot.Options.Count);
        Assert.All(snapshot.Options, option => Assert.Equal(RecruitmentOptionState.Available, option.State));
        Assert.Equal(new[] { "Alpha", "Beta", "Gamma" }, snapshot.Options.Select(o => o.GangName));
    }

    [Fact]
    public void Hire_AddsGangAndDebitsCash()
    {
        var gangs = CreateGangData();
        var dataService = new TestDataService(gangs);
        var rng = new SequenceRngService(new[] { 0, 0, 0, 0 });
        var service = new RecruitmentService(dataService, rng);
        var state = CreateGameState(500);
        var playerId = state.PrimaryPlayerId;

        var snapshot = service.EnsurePool(state, playerId, 1);
        var option = snapshot.Options[0];
        var player = (Player)state.Game.GetPlayer(playerId);
        var initialCash = player.Cash;

        var result = service.Hire(state, playerId, option.OptionId, "A1", 1);

        Assert.Equal(RecruitmentActionStatus.Success, result.Status);
        Assert.NotNull(result.GangId);
        Assert.True(state.Game.Gangs.ContainsKey(result.GangId!.Value));
        Assert.Equal(initialCash - option.HiringCost, player.Cash);
        Assert.Equal(RecruitmentOptionState.Hired, result.Option!.State);
        Assert.Equal(option.OptionId, result.Option.OptionId);
        Assert.Equal(RecruitmentOptionState.Hired, result.Pool.Options.First(o => o.OptionId == option.OptionId).State);
    }

    [Fact]
    public void RefreshPools_ReplacesDeclinedOptionsNextTurn()
    {
        var gangs = CreateGangData();
        var dataService = new TestDataService(gangs);
        var rng = new SequenceRngService(new[] { 0, 0, 0, 1 });
        var service = new RecruitmentService(dataService, rng);
        var state = CreateGameState();
        var playerId = state.PrimaryPlayerId;

        var initialPool = service.EnsurePool(state, playerId, 1);
        var declinedOption = initialPool.Options[1];

        var declineResult = service.Decline(state, playerId, declinedOption.OptionId, 1);
        Assert.Equal(RecruitmentActionStatus.Success, declineResult.Status);
        Assert.Equal(RecruitmentOptionState.Declined, declineResult.Option!.State);

        var refreshResults = service.RefreshPools(state, 2);
        var refresh = Assert.Single(refreshResults);

        Assert.True(refresh.HasChanges);
        var refreshedOption = refresh.Pool.Options.Single(o => o.SlotIndex == declinedOption.SlotIndex);
        Assert.Equal(RecruitmentOptionState.Available, refreshedOption.State);
        Assert.NotEqual(declinedOption.OptionId, refreshedOption.OptionId);
        Assert.Equal("Delta", refreshedOption.GangName);
    }

    private static GameState CreateGameState(int cash = 500)
    {
        var player = new Player(Guid.NewGuid(), "Player One", cash);
        var sector = new Sector("A1", CreateSiteData("A1 HQ"), player.Id);
        var game = new Game(new IPlayer[] { player }, new[] { sector });
        var scenario = new ScenarioConfig
        {
            Type = ScenarioType.KillEmAll,
            Name = "Test Scenario",
            Players = new List<ScenarioPlayerConfig>
            {
                new()
                {
                    Name = player.Name,
                    Kind = PlayerKind.Human,
                    StartingCash = cash,
                    HeadquartersSectorId = sector.Id,
                    StartingGangName = "Placeholder"
                }
            },
            Seed = 123
        };

        return new GameState(game, scenario, new List<IPlayer> { player }, 0, 123);
    }

    private static IReadOnlyList<GangData> CreateGangData()
    {
        return new List<GangData>
        {
            new() { Name = "Alpha", HiringCost = 100, UpkeepCost = 10 },
            new() { Name = "Beta", HiringCost = 120, UpkeepCost = 12 },
            new() { Name = "Gamma", HiringCost = 140, UpkeepCost = 14 },
            new() { Name = "Delta", HiringCost = 160, UpkeepCost = 16 },
            new() { Name = "Epsilon", HiringCost = 180, UpkeepCost = 18 }
        };
    }

    private static SiteData CreateSiteData(string name)
    {
        return new SiteData
        {
            Name = name,
            Cash = 2,
            Tolerance = 1
        };
    }

    private sealed class TestDataService(IReadOnlyList<GangData> gangs) : IDataService
    {
        private readonly IReadOnlyList<SiteData> _sites = new List<SiteData>
        {
            CreateSiteData("A1 HQ"),
            CreateSiteData("Neutral")
        };

        public Task<IReadOnlyList<GangData>> GetGangsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(gangs);
        }

        public IReadOnlyList<GangData> GetGangs()
        {
            return gangs;
        }

        public Task<IReadOnlyList<ItemData>> GetItemsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ItemData>>(Array.Empty<ItemData>());
        }

        public Task<IReadOnlyList<SiteData>> GetSitesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_sites);
        }

        public Task<IReadOnlyDictionary<int, ItemTypeData>> GetItemTypesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyDictionary<int, ItemTypeData>>(new Dictionary<int, ItemTypeData>());
        }

        public Task<SectorConfigurationData> GetSectorConfigurationAsync(CancellationToken cancellationToken = default)
        {
            var configuration = new SectorConfigurationData
            {
                Sectors = new List<SectorDefinitionData>
                {
                    new() { Id = "A1", SiteName = "A1 HQ" }
                }
            };

            return Task.FromResult(configuration);
        }
    }

    private sealed class SequenceRngService(IEnumerable<int> sequence) : IRngService
    {
        private readonly Queue<int> _sequence = new(sequence);

        public int Seed { get; private set; }

        public bool IsInitialised { get; private set; }

        public void Reset(int seed)
        {
            Seed = seed;
            IsInitialised = true;
        }

        public int NextInt()
        {
            IsInitialised = true;
            return _sequence.Count > 0 ? _sequence.Dequeue() : 0;
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;

            var value = NextInt();
            var range = maxExclusive - minInclusive;
            var normalized = Math.Abs(value) % range;
            return minInclusive + normalized;
        }

        public double NextDouble()
        {
            return 0.0;
        }

        public PercentileRollResult RollPercent()
        {
            IsInitialised = true;
            return new PercentileRollResult(1);
        }

        public DiceRollResult RollDice(int diceCount, int sides, int modifier = 0)
        {
            IsInitialised = true;
            var rolls = new int[Math.Max(1, diceCount)];
            for (var i = 0; i < rolls.Length; i++) rolls[i] = NextInt(1, sides <= 1 ? 2 : sides + 1);

            return new DiceRollResult(new ReadOnlyCollection<int>(rolls), modifier, "stub");
        }
    }
}