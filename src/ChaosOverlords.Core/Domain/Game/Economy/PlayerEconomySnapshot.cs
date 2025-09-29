using System;

namespace ChaosOverlords.Core.Domain.Game.Economy;

/// <summary>
/// Captures the financial adjustments applied to a single player during the upkeep phase.
/// </summary>
public sealed record PlayerEconomySnapshot(
    Guid PlayerId,
    string PlayerName,
    int StartingCash,
    int Upkeep,
    int SectorIncome,
    int SiteIncome,
    int NetChange,
    int EndingCash)
{
    /// <summary>
    /// Total income generated before upkeep costs are applied.
    /// </summary>
    public int TotalIncome => SectorIncome + SiteIncome;
}
