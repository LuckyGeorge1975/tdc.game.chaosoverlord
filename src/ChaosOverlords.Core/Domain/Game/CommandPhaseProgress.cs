namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
/// Tracks the current state of a command sub-phase within a turn.
/// </summary>
public sealed class CommandPhaseProgress
{
    public CommandPhaseProgress(CommandPhase phase, CommandPhaseState state)
    {
        Phase = phase;
        State = state;
    }

    /// <summary>
    /// The command phase represented by this entry.
    /// </summary>
    public CommandPhase Phase { get; }

    /// <summary>
    /// The current lifecycle state of the command phase.
    /// </summary>
    public CommandPhaseState State { get; internal set; }
}
