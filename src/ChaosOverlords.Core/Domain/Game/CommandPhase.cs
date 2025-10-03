namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
///     Represents the sub-phases that structure the command segment of a turn.
/// </summary>
public enum CommandPhase
{
    Instant,
    Combat,
    Transaction,
    Chaos,
    Movement,
    Control
}