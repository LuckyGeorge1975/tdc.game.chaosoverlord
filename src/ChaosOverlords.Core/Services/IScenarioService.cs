using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Scenario;

namespace ChaosOverlords.Core.Services;

/// <summary>
///     Provides scenario metadata and factory helpers for bootstrapping new campaigns.
/// </summary>
public interface IScenarioService
{
    /// <summary>
    ///     Creates a new game state instance based on the supplied configuration.
    /// </summary>
    Task<GameState> CreateNewGameAsync(ScenarioConfig config, CancellationToken cancellationToken = default);
}