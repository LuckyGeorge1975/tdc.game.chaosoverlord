using System.Collections.ObjectModel;

namespace ChaosOverlords.Core.Domain.Game.Economy;

/// <summary>
///     Identifies a named finance category used in the city and sector previews.
/// </summary>
public enum FinanceCategoryType
{
    Upkeep,
    NewRecruits,
    Research,
    Equipment,
    CityOfficials,
    SectorTax,
    SiteProtection,
    ChaosEstimate,
    CashAdjustment
}

/// <summary>
///     Represents an individual line item in the finance preview.
/// </summary>
public sealed class FinanceCategory
{
    public FinanceCategory(FinanceCategoryType type, string displayName, int amount)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be null or whitespace.", nameof(displayName));

        Type = type;
        DisplayName = displayName;
        Amount = amount;
    }

    public FinanceCategoryType Type { get; }

    public string DisplayName { get; }

    /// <summary>
    ///     Signed amount for this category. Expenses should be negative values, income positive values.
    /// </summary>
    public int Amount { get; }

    public bool IsExpense => Amount < 0;

    public bool IsIncome => Amount > 0;
}

/// <summary>
///     Finance breakdown for a single sector.
/// </summary>
public sealed class FinanceSectorProjection
{
    public FinanceSectorProjection(string sectorId, string displayName, IReadOnlyList<FinanceCategory> categories)
    {
        if (string.IsNullOrWhiteSpace(sectorId))
            throw new ArgumentException("Sector id cannot be null or whitespace.", nameof(sectorId));

        SectorId = sectorId;
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        Categories =
            new ReadOnlyCollection<FinanceCategory>((categories ?? throw new ArgumentNullException(nameof(categories)))
                .ToList());
    }

    public string SectorId { get; }

    public string DisplayName { get; }

    public IReadOnlyList<FinanceCategory> Categories { get; }

    public int NetChange => Categories.Where(category => category.Type != FinanceCategoryType.CashAdjustment)
        .Sum(category => category.Amount);
}

/// <summary>
///     Aggregated finance preview for the active player.
/// </summary>
public sealed class FinanceProjection
{
    public FinanceProjection(Guid playerId, string playerName, IReadOnlyList<FinanceCategory> cityCategories,
        IReadOnlyList<FinanceSectorProjection> sectors)
    {
        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));

        if (string.IsNullOrWhiteSpace(playerName))
            throw new ArgumentException("Player name cannot be null or whitespace.", nameof(playerName));

        PlayerId = playerId;
        PlayerName = playerName;
        CityCategories =
            new ReadOnlyCollection<FinanceCategory>(
                (cityCategories ?? throw new ArgumentNullException(nameof(cityCategories))).ToList());
        Sectors = new ReadOnlyCollection<FinanceSectorProjection>(
            (sectors ?? throw new ArgumentNullException(nameof(sectors))).ToList());
    }

    public Guid PlayerId { get; }

    public string PlayerName { get; }

    public IReadOnlyList<FinanceCategory> CityCategories { get; }

    public IReadOnlyList<FinanceSectorProjection> Sectors { get; }

    public int NetCashAdjustment => CityCategories
        .Where(category => category.Type != FinanceCategoryType.CashAdjustment).Sum(category => category.Amount);
}