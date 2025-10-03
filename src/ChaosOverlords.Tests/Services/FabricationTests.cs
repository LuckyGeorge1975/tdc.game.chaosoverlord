using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Actions;
using ChaosOverlords.Core.Domain.Game.Commands;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Domain.Game.Events;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Tests.Services;

public sealed class FabricationTests
{
    [Fact]
    public void QueueFabricate_ShowsCost_In_FinancePreview()
    {
        var (state, player, gang) = CreateState();
        var data = new FakeDataService(new[]
        {
            new ItemData { Name = "Laser Rifle", FabricationCost = 30, ResearchCost = 0, TechLevel = 1 }
        });

        var preview = new FinancePreviewService(data);
        var queueSvc = new CommandQueueService();
        var result = queueSvc.QueueFabricate(state, player.Id, gang.Id, "Laser Rifle", 1);
        Assert.Equal(CommandQueueRequestStatus.Success, result.Status);

        var projection = preview.BuildProjection(state, player.Id);
        var equipment = projection.CityCategories.Single(c => c.Type == FinanceCategoryType.Equipment);
        Assert.Equal(-30, equipment.Amount);
    }

    [Fact]
    public void ExecuteFabricate_DebitsCash_And_AddsToWarehouse()
    {
        var (state, player, gang) = CreateState(100);
        var data = new FakeDataService(new[]
        {
            new ItemData { Name = "Armor Vest", FabricationCost = 40, ResearchCost = 0, TechLevel = 1 }
        });

        var writer = new RecordingEventWriter();
        var rng = new DeterministicRngService();
        var research = new ResearchService(data);
        var equipment = new EquipmentService();
        var resolver = new CommandResolutionService(writer, rng, research, data, equipment);

        var queue = state.Commands.GetOrCreate(player.Id);
        queue.SetCommand(new FabricateCommand(Guid.NewGuid(), player.Id, gang.Id, 1, "Armor Vest"));

        var beforeCash = player.Cash;
        var report = resolver.Execute(state, player.Id, 1);
        var entry = Assert.Single(report.Entries);
        Assert.Equal(CommandExecutionStatus.Completed, entry.Status);
        Assert.Equal(beforeCash - 40, player.Cash);
        Assert.Contains(state.Warehouse.GetOrCreate(player.Id).Items.Values, i => i.Name == "Armor Vest");
    }

    private static (GameState State, Player Player, Gang Gang) CreateState(int startingCash = 50)
    {
        var player = new Player(Guid.NewGuid(), "Buyer", startingCash);
        var gangData = new GangData { Name = "Crew" };
        var gang = new Gang(Guid.NewGuid(), gangData, player.Id, "A1");

        var sector = new Sector("A1", new SiteData { Name = "HQ" }, player.Id);
        var game = new Game(new IPlayer[] { player }, new[] { sector }, new[] { gang });
        var scenario = new ScenarioConfig
        {
            Type = ScenarioType.KillEmAll,
            Name = "Fabrication Test",
            Players = new List<ScenarioPlayerConfig>
            {
                new()
                {
                    Name = player.Name,
                    Kind = PlayerKind.Human,
                    StartingCash = startingCash,
                    HeadquartersSectorId = "A1",
                    StartingGangName = gangData.Name
                }
            },
            MapSectorIds = new List<string> { "A1" },
            Seed = 1
        };

        var state = new GameState(game, scenario, new List<IPlayer> { player }, 0, 1);
        return (state, player, gang);
    }

    private sealed class FakeDataService : IDataService
    {
        private readonly IReadOnlyList<ItemData> _items;

        public FakeDataService(IEnumerable<ItemData> items)
        {
            _items = items.ToList();
        }

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
            return Task.FromResult(_items);
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

    private sealed class RecordingEventWriter : ITurnEventWriter
    {
        public List<(int TurnNumber, TurnPhase Phase, CommandPhase? CommandPhase, TurnEventType Type, string Description
            )> Events { get; } = new();

        public void Write(int turnNumber, TurnPhase phase, TurnEventType type, string description,
            CommandPhase? commandPhase = null)
        {
            Events.Add((turnNumber, phase, commandPhase, type, description));
        }

        public void WriteEconomy(int turnNumber, TurnPhase phase, PlayerEconomySnapshot snapshot)
        {
        }

        public void WriteAction(ActionResult result)
        {
        }
    }
}