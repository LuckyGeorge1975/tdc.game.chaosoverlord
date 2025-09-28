using System.Collections.ObjectModel;

namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
/// Captures the mutable state for a player/overlord during a campaign (cash, owned gangs, etc.).
/// </summary>
public sealed class Player
{
    private readonly List<Guid> _gangIds = new();

    public Player(Guid id, string name, int cash = 0, IEnumerable<Guid>? gangIds = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Player name cannot be null or whitespace.", nameof(name));
        }

        Id = id;
        Name = name;
        Cash = cash;

        if (gangIds is not null)
        {
            _gangIds.AddRange(gangIds);
        }
    }

    public Guid Id { get; }

    public string Name { get; }

    public int Cash { get; private set; }

    public IReadOnlyCollection<Guid> GangIds => new ReadOnlyCollection<Guid>(_gangIds);

    /// <summary>
    /// Applies income to the player's treasury (taxes, site bonuses, etc.).
    /// </summary>
    public void Credit(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be non-negative when crediting cash.");
        }

        Cash += amount;
    }

    /// <summary>
    /// Spends cash on upkeep, recruitment, or other campaign actions.
    /// </summary>
    public void Debit(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be non-negative when debiting cash.");
        }

        Cash -= amount;
    }

    /// <summary>
    /// Associates a gang with the player roster (used when hiring or transferring control).
    /// </summary>
    public void AssignGang(Guid gangId)
    {
        if (!_gangIds.Contains(gangId))
        {
            _gangIds.Add(gangId);
        }
    }

    /// <summary>
    /// Removes a gang from the player's roster.
    /// </summary>
    public bool RemoveGang(Guid gangId) => _gangIds.Remove(gangId);
}
