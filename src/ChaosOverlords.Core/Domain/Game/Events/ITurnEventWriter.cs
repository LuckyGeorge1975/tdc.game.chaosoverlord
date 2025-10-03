using ChaosOverlords.Core.Domain.Game.Actions;
using ChaosOverlords.Core.Domain.Game.Economy;

namespace ChaosOverlords.Core.Domain.Game.Events;

/// <summary>
///     Provides a higher-level API for emitting turn events with consistent metadata.
/// </summary>
public interface ITurnEventWriter
{
    /// <summary>
    ///     Writes a generic turn event with the supplied metadata.
    /// </summary>
    void Write(int turnNumber, TurnPhase phase, TurnEventType type, string description,
        CommandPhase? commandPhase = null);

    /// <summary>
    ///     Writes an economy-specific event describing upkeep results for a player.
    /// </summary>
    void WriteEconomy(int turnNumber, TurnPhase phase, PlayerEconomySnapshot snapshot);

    /// <summary>
    ///     Writes an action resolution event with dice information.
    /// </summary>
    void WriteAction(ActionResult result);
}