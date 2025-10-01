using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Economy;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Default implementation that builds finance preview data using the current game state.
/// </summary>
public sealed class FinancePreviewService : IFinancePreviewService
{
    private static readonly IReadOnlyList<FinanceCategoryType> CategoryOrder = new[]
    {
        FinanceCategoryType.Upkeep,
        FinanceCategoryType.NewRecruits,
        FinanceCategoryType.Equipment,
        FinanceCategoryType.CityOfficials,
        FinanceCategoryType.SectorTax,
        FinanceCategoryType.SiteProtection,
        FinanceCategoryType.ChaosEstimate
    };

    public FinanceProjection BuildProjection(GameState gameState, Guid playerId)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (playerId == Guid.Empty)
        {
            throw new ArgumentException("Player id must be provided.", nameof(playerId));
        }

        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetPlayer(playerId, out var player) || player is null)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Player '{0}' was not found in the game state.", playerId));
        }

    var cityCategories = BuildCityCategories(gameState, playerId);
        var sectors = BuildSectorProjections(gameState, playerId);

        var cashAdjustment = cityCategories.Where(category => category.Type != FinanceCategoryType.CashAdjustment).Sum(category => category.Amount);
        var allCategories = new List<FinanceCategory>(cityCategories)
        {
            new(FinanceCategoryType.CashAdjustment, "Cash Adjustment", cashAdjustment)
        };

        return new FinanceProjection(playerId, player.Name, allCategories, sectors);
    }

    private static List<FinanceCategory> BuildCityCategories(GameState gameState, Guid playerId)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        var gangs = game.Gangs.Values.Where(g => g.OwnerId == playerId).ToList();
        var sectors = game.Sectors.Values.Where(s => s.ControllingPlayerId == playerId).ToList();

        var upkeep = -gangs.Sum(g => g.Data.UpkeepCost);
        var newRecruits = 0;
        var equipment = 0;
        var cityOfficials = 0;
    var sectorTax = sectors.Sum(s => s.Site?.Cash ?? 0);
    var siteProtection = 0;
        var chaosEstimate = sectors.Sum(s => s.ProjectedChaos);

        var categories = new Dictionary<FinanceCategoryType, FinanceCategory>
        {
            [FinanceCategoryType.Upkeep] = new(FinanceCategoryType.Upkeep, "Upkeep", upkeep),
            [FinanceCategoryType.NewRecruits] = new(FinanceCategoryType.NewRecruits, "New Recruits", -newRecruits),
            [FinanceCategoryType.Equipment] = new(FinanceCategoryType.Equipment, "Equipment", -equipment),
            [FinanceCategoryType.CityOfficials] = new(FinanceCategoryType.CityOfficials, "City Officials", -cityOfficials),
            [FinanceCategoryType.SectorTax] = new(FinanceCategoryType.SectorTax, "Sector Tax", sectorTax),
            [FinanceCategoryType.SiteProtection] = new(FinanceCategoryType.SiteProtection, "Site Protection", siteProtection),
            [FinanceCategoryType.ChaosEstimate] = new(FinanceCategoryType.ChaosEstimate, "Chaos (Estimate)", chaosEstimate)
        };

        return CategoryOrder.Select(type => categories[type]).ToList();
    }

    private static List<FinanceSectorProjection> BuildSectorProjections(GameState gameState, Guid playerId)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        var sectors = game.Sectors.Values.Where(s => s.ControllingPlayerId == playerId).OrderBy(s => s.Id, StringComparer.Ordinal).ToList();
        var gangsBySector = game.Gangs.Values.Where(g => g.OwnerId == playerId).GroupBy(g => g.SectorId).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

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
}
