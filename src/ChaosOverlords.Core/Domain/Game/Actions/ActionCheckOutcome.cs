namespace ChaosOverlords.Core.Domain.Game.Actions;

/// <summary>
///     Describes the outcome of resolving an action check.
/// </summary>
public enum ActionCheckOutcome
{
    /// <summary>
    ///     The action automatically failed regardless of modifiers or dice results.
    /// </summary>
    AutomaticFailure,

    /// <summary>
    ///     The action failed after evaluating the effective chance against the roll.
    /// </summary>
    Failure,

    /// <summary>
    ///     The action succeeded after evaluating the effective chance against the roll.
    /// </summary>
    Success,

    /// <summary>
    ///     The action automatically succeeded regardless of modifiers or dice results.
    /// </summary>
    AutomaticSuccess
}