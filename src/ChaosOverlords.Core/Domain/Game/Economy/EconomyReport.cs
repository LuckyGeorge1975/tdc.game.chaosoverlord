using System.Collections.ObjectModel;
using System.Linq;

namespace ChaosOverlords.Core.Domain.Game.Economy;

/// <summary>
/// Aggregates the upkeep processing outcome for all players in a given turn.
/// </summary>
public sealed class EconomyReport
{
    public EconomyReport(int turnNumber, IReadOnlyList<PlayerEconomySnapshot> playerSnapshots)
    {
        if (playerSnapshots is null)
        {
            throw new ArgumentNullException(nameof(playerSnapshots));
        }

        TurnNumber = turnNumber;
        PlayerSnapshots = new ReadOnlyCollection<PlayerEconomySnapshot>(playerSnapshots.ToList());
    }

    /// <summary>
    /// Turn index (1-based) for which the report was generated.
    /// </summary>
    public int TurnNumber { get; }

    /// <summary>
    /// Financial breakdown per player.
    /// </summary>
    public IReadOnlyList<PlayerEconomySnapshot> PlayerSnapshots { get; }

    public int TotalUpkeep => PlayerSnapshots.Sum(p => p.Upkeep);

    public int TotalSectorIncome => PlayerSnapshots.Sum(p => p.SectorIncome);

    public int TotalSiteIncome => PlayerSnapshots.Sum(p => p.SiteIncome);

    public int TotalNetChange => PlayerSnapshots.Sum(p => p.NetChange);
}
