namespace ChaosOverlords.Core.Domain.Game.Recruitment;

/// <summary>
///     Represents the current availability of a recruitment option within a player's pool.
/// </summary>
public enum RecruitmentOptionState
{
    /// <summary>
    ///     The option is available for hiring during the current turn.
    /// </summary>
    Available,

    /// <summary>
    ///     The option was declined this turn and will be replaced at the start of the next turn.
    /// </summary>
    Declined,

    /// <summary>
    ///     The option was hired this turn and will be replaced at the start of the next turn.
    /// </summary>
    Hired
}