namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
///     Represents the major phases that structure a single turn in the campaign loop.
/// </summary>
public enum TurnPhase
{
    Upkeep,
    Command,
    Execution,
    Hire,
    Elimination
}