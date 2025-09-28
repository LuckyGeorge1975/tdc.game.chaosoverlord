using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Domain.Players;

/// <summary>
/// Contract shared by all player controllers participating in a campaign.
/// </summary>
public interface IPlayer
{
    Guid Id { get; }

    string Name { get; }

    int Cash { get; }

    IReadOnlyCollection<Guid> GangIds { get; }

    void Credit(int amount);

    void Debit(int amount);

    void AssignGang(Guid gangId);

    bool RemoveGang(Guid gangId);

    /// <summary>
    /// Executes the player's turn. Human players wait for input, AI players run internal logic.
    /// </summary>
    Task ExecuteTurnAsync(GameStateManager manager, CancellationToken cancellationToken);
}
