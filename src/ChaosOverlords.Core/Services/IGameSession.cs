using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Scenario;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Provides access to the active campaign session, including the game state and manager utilities.
/// </summary>
public interface IGameSession
{
    /// <summary>
    /// Indicates whether a game has been successfully initialised.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// The scenario configuration used to bootstrap the current session.
    /// </summary>
    ScenarioConfig Scenario { get; }

    /// <summary>
    /// The runtime game state for the active campaign.
    /// </summary>
    GameState GameState { get; }

    /// <summary>
    /// Convenience access to the state manager coordinating player execution.
    /// </summary>
    GameStateManager Manager { get; }

    /// <summary>
    /// Ensures that a session is prepared. Subsequent calls are ignored.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
