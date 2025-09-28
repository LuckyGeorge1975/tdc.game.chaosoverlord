using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Domain.Players;

/// <summary>
/// Human-controlled player. Execution waits for UI workflow (placeholder pending integration).
/// </summary>
public sealed class Player : PlayerBase
{
    public Player(Guid id, string name, int cash = 0, IEnumerable<Guid>? gangIds = null)
        : base(id, name, cash, gangIds)
    {
    }

    public override Task ExecuteTurnAsync(GameStateManager manager, CancellationToken cancellationToken)
    {
        // Placeholder: actual implementation will coordinate with UI input workflow.
        return Task.CompletedTask;
    }
}
