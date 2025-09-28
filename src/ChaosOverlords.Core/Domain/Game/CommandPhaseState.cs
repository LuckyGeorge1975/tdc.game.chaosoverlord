namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
/// Represents the lifecycle state for an individual command sub-phase.
/// </summary>
public enum CommandPhaseState
{
    Upcoming,
    Active,
    Completed
}
