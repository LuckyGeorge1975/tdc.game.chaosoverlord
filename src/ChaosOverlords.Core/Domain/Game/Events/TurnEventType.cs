namespace ChaosOverlords.Core.Domain.Game.Events;

/// <summary>
/// Categorises events that occur during the execution of a turn.
/// </summary>
public enum TurnEventType
{
    TurnStarted,
    PhaseAdvanced,
    CommandPhaseAdvanced,
    TurnCompleted,
    Information
}
