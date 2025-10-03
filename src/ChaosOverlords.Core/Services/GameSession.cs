using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Scenario;

namespace ChaosOverlords.Core.Services;

/// <summary>
///     Tracks the lifetime of the currently active campaign session.
/// </summary>
public sealed class GameSession : IGameSession
{
    private readonly IDefaultScenarioProvider _scenarioProvider;
    private readonly IScenarioService _scenarioService;
    private GameState? _gameState;
    private GameStateManager? _manager;
    private ScenarioConfig? _scenario;

    public GameSession(
        IScenarioService scenarioService,
        IDefaultScenarioProvider scenarioProvider)
    {
        _scenarioService = scenarioService ?? throw new ArgumentNullException(nameof(scenarioService));
        _scenarioProvider = scenarioProvider ?? throw new ArgumentNullException(nameof(scenarioProvider));
    }

    public bool IsInitialized { get; private set; }

    public ScenarioConfig Scenario =>
        _scenario ?? throw new InvalidOperationException("Game session has not been initialised.");

    public GameState GameState =>
        _gameState ?? throw new InvalidOperationException("Game session has not been initialised.");

    public GameStateManager Manager =>
        _manager ?? throw new InvalidOperationException("Game session has not been initialised.");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsInitialized) return;

        var scenario = _scenarioProvider.CreateScenario();
        var state = await _scenarioService.CreateNewGameAsync(scenario, cancellationToken).ConfigureAwait(false);

        _scenario = scenario;
        _gameState = state;
        _manager = new GameStateManager(state);

        IsInitialized = true;
    }
}