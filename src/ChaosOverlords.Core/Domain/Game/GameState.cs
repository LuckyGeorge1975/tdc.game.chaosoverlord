using System.Collections.ObjectModel;
using ChaosOverlords.Core.Domain.Game.Recruitment;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;

namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
/// Encapsulates the full runtime snapshot for an active campaign.
/// </summary>
public sealed class GameState
{
    private readonly List<IPlayer> _playerOrder;
    private readonly int _startingPlayerIndex;
    private int _currentPlayerIndex;

    public GameState(
        Game game,
        ScenarioConfig scenario,
        IReadOnlyList<IPlayer> playerOrder,
        int startingPlayerIndex = 0,
        int randomSeed = 0)
    {
        Game = game ?? throw new ArgumentNullException(nameof(game));
        Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
        if (playerOrder is null)
        {
            throw new ArgumentNullException(nameof(playerOrder));
        }

        if (playerOrder.Count == 0)
        {
            throw new ArgumentException("At least one player must be registered.", nameof(playerOrder));
        }

        if (startingPlayerIndex < 0 || startingPlayerIndex >= playerOrder.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(startingPlayerIndex), startingPlayerIndex, "Starting player index is out of bounds.");
        }

        _playerOrder = new List<IPlayer>(playerOrder);
        _startingPlayerIndex = startingPlayerIndex;
        _currentPlayerIndex = startingPlayerIndex;

        RandomSeed = randomSeed;

        PrimaryPlayer = _playerOrder.OfType<Player>().FirstOrDefault() ?? _playerOrder[0];
    }

    /// <summary>
    /// Aggregated runtime entities (players, gangs, items, sectors).
    /// </summary>
    public Game Game { get; }

    /// <summary>
    /// Scenario metadata that produced this campaign.
    /// </summary>
    public ScenarioConfig Scenario { get; }

    /// <summary>
    /// Primary (human-controlled) player.
    /// </summary>
    public IPlayer PrimaryPlayer { get; }

    /// <summary>
    /// Seed that initialises the deterministic RNG for this game instance.
    /// </summary>
    public int RandomSeed { get; }

    /// <summary>
    /// Recruitment pools tracking available candidates for each player.
    /// </summary>
    public RecruitmentState Recruitment { get; } = new();

    /// <summary>
    /// Identifier of the primary player.
    /// </summary>
    public Guid PrimaryPlayerId => PrimaryPlayer.Id;

    /// <summary>
    /// Currently active player in the rotation.
    /// </summary>
    public IPlayer CurrentPlayer => _playerOrder[_currentPlayerIndex];

    /// <summary>
    /// Turn order for the campaign.
    /// </summary>
    public IReadOnlyList<IPlayer> PlayerOrder => new ReadOnlyCollection<IPlayer>(_playerOrder);

    /// <summary>
    /// Zero-based index of the active player within <see cref="PlayerOrder"/>.
    /// </summary>
    public int CurrentPlayerIndex => _currentPlayerIndex;

    /// <summary>
    /// Current turn number, starting at 1.
    /// </summary>
    public int TurnNumber { get; private set; } = 1;

    /// <summary>
    /// Advances the global turn counter.
    /// </summary>
    public void AdvanceTurn()
    {
        if (TurnNumber == int.MaxValue)
        {
            throw new InvalidOperationException("Turn counter overflow.");
        }

        TurnNumber++;
        _currentPlayerIndex = _startingPlayerIndex;
    }

    /// <summary>
    /// Advances to the next player in the rotation.
    /// </summary>
    public void AdvanceToNextPlayer()
    {
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _playerOrder.Count;
    }

    /// <summary>
    /// Forces the active player to a specific slot (used by the game manager when resuming saves or applying skips).
    /// </summary>
    public void SetCurrentPlayer(Guid playerId)
    {
        var index = _playerOrder.FindIndex(p => p.Id == playerId);
        if (index < 0)
        {
            throw new InvalidOperationException($"Player '{playerId}' is not registered in the turn order.");
        }

        _currentPlayerIndex = index;
    }
}
