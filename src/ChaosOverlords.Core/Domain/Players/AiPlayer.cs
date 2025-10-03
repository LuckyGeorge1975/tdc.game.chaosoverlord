using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Domain.Players;

/// <summary>
///     AI-controlled player stub. Actual decision logic will be implemented in later phases.
/// </summary>
public sealed class AiPlayer(
    Guid id,
    string name,
    AiDifficulty difficulty,
    int cash = 0,
    IEnumerable<Guid>? gangIds = null)
    : PlayerBase(id, name, cash, gangIds)
{
    public AiDifficulty Difficulty { get; } = difficulty;

    public override Task ExecuteTurnAsync(GameStateManager manager, CancellationToken cancellationToken)
    {
        // Placeholder: invoke AI logic here in the future.
        return Task.CompletedTask;
    }
}