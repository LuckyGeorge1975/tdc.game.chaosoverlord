using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Domain.Players;

/// <summary>
/// Placeholder for remote/network-controlled player integration.
/// </summary>
public sealed class NetworkPlayer : PlayerBase
{
    public NetworkPlayer(Guid id, string name, int cash = 0, IEnumerable<Guid>? gangIds = null)
        : base(id, name, cash, gangIds)
    {
    }

    public override Task ExecuteTurnAsync(GameStateManager manager, CancellationToken cancellationToken)
    {
        // Placeholder: handle incoming network commands in future iterations.
        return Task.CompletedTask;
    }
}
