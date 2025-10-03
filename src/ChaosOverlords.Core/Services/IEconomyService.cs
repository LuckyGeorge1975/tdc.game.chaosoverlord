using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Economy;

namespace ChaosOverlords.Core.Services;

/// <summary>
///     Applies upkeep and income adjustments for the players participating in a campaign.
/// </summary>
public interface IEconomyService
{
    /// <summary>
    ///     Processes upkeep for the supplied game state and returns a detailed report.
    /// </summary>
    /// <param name="gameState">The active game state to mutate.</param>
    /// <param name="turnNumber">The turn index (1-based) associated with the processing.</param>
    /// <returns>A snapshot of the financial adjustments per player.</returns>
    EconomyReport ApplyUpkeep(GameState gameState, int turnNumber);
}