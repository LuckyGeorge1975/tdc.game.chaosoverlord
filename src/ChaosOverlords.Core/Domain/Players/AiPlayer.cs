using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Domain.Players;

/// <summary>
/// AI-controlled player stub. Actual decision logic will be implemented in later phases.
/// </summary>
public sealed class AiPlayer : PlayerBase
{
    public AiPlayer(Guid id, string name, AiDifficulty difficulty, int cash = 0, IEnumerable<Guid>? gangIds = null)
        : base(id, name, cash, gangIds)
    {
        Difficulty = difficulty;
    }

    public AiDifficulty Difficulty { get; }

    public override Task ExecuteTurnAsync(GameStateManager manager, CancellationToken cancellationToken)
    {
        // Placeholder: invoke AI logic here in the future.
        return Task.CompletedTask;
    }
}
