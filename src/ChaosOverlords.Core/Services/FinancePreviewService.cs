using System.Globalization;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Commands;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Core.Services;

/// <summary>
///     Default implementation that builds finance preview data using the current game state.
/// </summary>
public sealed class FinancePreviewService : IFinancePreviewService
{
    private static readonly IReadOnlyList<FinanceCategoryType> CategoryOrder = new[]
    {
        FinanceCategoryType.Upkeep,
        FinanceCategoryType.NewRecruits,
        FinanceCategoryType.Research,
        FinanceCategoryType.Equipment,
        FinanceCategoryType.CityOfficials,
        FinanceCategoryType.SectorTax,
        FinanceCategoryType.SiteProtection,
        FinanceCategoryType.ChaosEstimate
    };

    private readonly IDataService _dataService;
    private readonly IResearchService _researchService;

    public FinancePreviewService() : this(new EmptyDataService(), new ResearchService())
    {
    }

    public FinancePreviewService(IDataService dataService, IResearchService researchService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _researchService = researchService ?? throw new ArgumentNullException(nameof(researchService));
    }

    // Backward-compatible overload used in existing tests and app wiring
    public FinancePreviewService(IDataService dataService)
        : this(dataService, new ResearchService(dataService))
    {
    }

    public FinanceProjection BuildProjection(GameState gameState, Guid playerId)
    {
        if (gameState is null) throw new ArgumentNullException(nameof(gameState));

        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));

        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetPlayer(playerId, out var player) || player is null)
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                "Player '{0}' was not found in the game state.", playerId));

        var cityCategories = BuildCityCategories(gameState, playerId);
        var sectors = BuildSectorProjections(gameState, playerId);

        var cashAdjustment = cityCategories.Where(category => category.Type != FinanceCategoryType.CashAdjustment)
            .Sum(category => category.Amount);
        var allCategories = new List<FinanceCategory>(cityCategories)
        {
            new(FinanceCategoryType.CashAdjustment, "Cash Adjustment", cashAdjustment)
        };

        return new FinanceProjection(playerId, player.Name, allCategories, sectors);
    }

    private List<FinanceCategory> BuildCityCategories(GameState gameState, Guid playerId)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        var gangs = game.Gangs.Values.Where(g => g.OwnerId == playerId).ToList();
        var sectors = game.Sectors.Values.Where(s => s.ControllingPlayerId == playerId).ToList();

        var upkeep = -gangs.Sum(g => g.Data.UpkeepCost);
        var newRecruits = 0;
        var equipment = EstimateEquipmentSpending(gameState, playerId);
        var research = EstimateResearchInvestment(gameState, playerId);
        var cityOfficials = 0;
        var sectorTax = sectors.Sum(s => s.Site?.Cash ?? 0);
        var siteProtection = 0;
        var chaosEstimate = sectors.Sum(s => s.ProjectedChaos);

        var categories = new Dictionary<FinanceCategoryType, FinanceCategory>
        {
            [FinanceCategoryType.Upkeep] = new(FinanceCategoryType.Upkeep, "Upkeep", upkeep),
            [FinanceCategoryType.NewRecruits] = new(FinanceCategoryType.NewRecruits, "New Recruits", -newRecruits),
            [FinanceCategoryType.Equipment] = new(FinanceCategoryType.Equipment, "Equipment", -equipment),
            [FinanceCategoryType.Research] = new(FinanceCategoryType.Research, "Research", -research),
            [FinanceCategoryType.CityOfficials] =
                new(FinanceCategoryType.CityOfficials, "City Officials", -cityOfficials),
            [FinanceCategoryType.SectorTax] = new(FinanceCategoryType.SectorTax, "Sector Tax", sectorTax),
            [FinanceCategoryType.SiteProtection] =
                new(FinanceCategoryType.SiteProtection, "Site Protection", siteProtection),
            [FinanceCategoryType.ChaosEstimate] =
                new(FinanceCategoryType.ChaosEstimate, "Chaos (Estimate)", chaosEstimate)
        };

        return CategoryOrder.Select(type => categories[type]).ToList();
    }

    private static List<FinanceSectorProjection> BuildSectorProjections(GameState gameState, Guid playerId)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        var sectors = game.Sectors.Values.Where(s => s.ControllingPlayerId == playerId)
            .OrderBy(s => s.Id, StringComparer.Ordinal).ToList();
        var gangsBySector = game.Gangs.Values.Where(g => g.OwnerId == playerId).GroupBy(g => g.SectorId)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        var projections = new List<FinanceSectorProjection>();
        foreach (var sector in sectors)
        {
            var displayName = sector.Site is null
                ? sector.Id
                : string.Format(CultureInfo.CurrentCulture, "{0} – {1}", sector.Id, sector.Site.Name);

            var upkeepCost = gangsBySector.TryGetValue(sector.Id, out var gangsInSector)
                ? gangsInSector.Sum(g => g.Data.UpkeepCost)
                : 0;

            var categories = new List<FinanceCategory>
            {
                new(FinanceCategoryType.Upkeep, "Upkeep", -upkeepCost),
                new(FinanceCategoryType.NewRecruits, "New Recruits", 0),
                new(FinanceCategoryType.Equipment, "Equipment", 0),
                new(FinanceCategoryType.CityOfficials, "City Officials", 0),
                new(FinanceCategoryType.SectorTax, "Sector Tax", sector.Site?.Cash ?? 0),
                new(FinanceCategoryType.SiteProtection, "Site Protection", 0),
                new(FinanceCategoryType.ChaosEstimate, "Chaos (Estimate)", sector.ProjectedChaos)
            };

            projections.Add(new FinanceSectorProjection(sector.Id, displayName, categories));
        }

        return projections;
    }

    private int EstimateEquipmentSpending(GameState gameState, Guid playerId)
    {
        if (!gameState.Commands.TryGet(playerId, out var queue) || queue is null || queue.Commands.Count == 0) return 0;

        var fabricateCommands = queue.Commands.OfType<FabricateCommand>().ToList();
        if (fabricateCommands.Count == 0) return 0;

        var items = _dataService.GetItemsAsync().GetAwaiter().GetResult();
        var priceByName = items.ToDictionary(i => i.Name, i => Math.Max(0, i.FabricationCost), StringComparer.Ordinal);
        var total = 0;
        foreach (var cmd in fabricateCommands)
            if (priceByName.TryGetValue(cmd.ItemName, out var price))
                total += price;

        return total;
    }

    private int EstimateResearchInvestment(GameState gameState, Guid playerId)
    {
        // For Task 18 scope, treat research as non-cash investment but show estimated progress as pseudo-cost 0.
        // Later phases may convert this to actual costs (labs, consumables). Keeping it visible as a category.
        var preview = _researchService.BuildPreview(gameState, playerId);
        _ = preview; // currently unused value; category amount remains 0 to avoid double-counting cash.
        return 0;
    }
}

internal sealed class EmptyDataService : IDataService
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
        return Task.FromResult<IReadOnlyList<ItemData>>(Array.Empty<ItemData>());
    }

    public Task<IReadOnlyList<SiteData>> GetSitesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<SiteData>>(Array.Empty<SiteData>());
    }

    public Task<IReadOnlyDictionary<int, ItemTypeData>> GetItemTypesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyDictionary<int, ItemTypeData>>(
            new Dictionary<int, ItemTypeData>());
    }

    public Task<SectorConfigurationData> GetSectorConfigurationAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SectorConfigurationData());
    }
}