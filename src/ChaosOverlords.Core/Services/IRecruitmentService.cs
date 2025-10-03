using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Recruitment;

namespace ChaosOverlords.Core.Services;

/// <summary>
///     Provides recruitment pool management and hire/decline operations for the hire phase.
/// </summary>
public interface IRecruitmentService
{
    /// <summary>
    ///     Ensures the player's pool is initialised and refreshed for the supplied turn.
    /// </summary>
    RecruitmentPoolSnapshot EnsurePool(GameState gameState, Guid playerId, int turnNumber);

    /// <summary>
    ///     Refreshes all player pools at the start of a turn, returning snapshots for logging.
    /// </summary>
    IReadOnlyList<RecruitmentRefreshResult> RefreshPools(GameState gameState, int turnNumber);

    /// <summary>
    ///     Attempts to hire the specified recruitment option into the indicated sector.
    /// </summary>
    RecruitmentHireResult Hire(GameState gameState, Guid playerId, Guid optionId, string sectorId, int turnNumber);

    /// <summary>
    ///     Declines the specified option, marking it for replacement on the next turn.
    /// </summary>
    RecruitmentDeclineResult Decline(GameState gameState, Guid playerId, Guid optionId, int turnNumber);
}