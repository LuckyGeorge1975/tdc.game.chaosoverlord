using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Domain.Players;

/// <summary>
/// Base implementation providing common bookkeeping for all player controllers.
/// </summary>
public abstract class PlayerBase : IPlayer
{
    private readonly List<Guid> _gangIds = new();

    protected PlayerBase(Guid id, string name, int cash = 0, IEnumerable<Guid>? gangIds = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Player id cannot be empty.", nameof(id));
        }

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

    public void Credit(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be non-negative when crediting cash.");
        }

        Cash += amount;
    }

    public void Debit(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be non-negative when debiting cash.");
        }

        Cash -= amount;
    }

    public void AssignGang(Guid gangId)
    {
        if (!_gangIds.Contains(gangId))
        {
            _gangIds.Add(gangId);
        }
    }

    public bool RemoveGang(Guid gangId) => _gangIds.Remove(gangId);

    public abstract Task ExecuteTurnAsync(GameStateManager manager, CancellationToken cancellationToken);
}
