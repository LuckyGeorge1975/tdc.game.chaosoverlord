using ChaosOverlords.Core.Domain.Players;

namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
///     Coordinates turn execution across the players registered in a <see cref="GameState" />.
/// </summary>
public sealed class GameStateManager(GameState gameState)
{
    public GameState GameState { get; } = gameState ?? throw new ArgumentNullException(nameof(gameState));

    public IPlayer CurrentPlayer => GameState.CurrentPlayer;

    public async Task ExecuteCurrentPlayerAsync(CancellationToken cancellationToken = default)
    {
        await CurrentPlayer.ExecuteTurnAsync(this, cancellationToken).ConfigureAwait(false);
    }

    public void AdvanceToNextPlayer()
    {
        GameState.AdvanceToNextPlayer();
    }

    public void AdvanceTurn()
    {
        GameState.AdvanceTurn();
    }

    public void SetCurrentPlayer(Guid playerId)
    {
        GameState.SetCurrentPlayer(playerId);
    }
}