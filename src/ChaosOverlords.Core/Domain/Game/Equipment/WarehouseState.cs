namespace ChaosOverlords.Core.Domain.Game.Equipment;

/// <summary>
///     Tracks per-player item inventory (warehouse) outside of gangs.
/// </summary>
public sealed class WarehouseState
{
    private readonly Dictionary<Guid, PlayerWarehouse> _players = new();

    public PlayerWarehouse GetOrCreate(Guid playerId)
    {
        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));

        if (!_players.TryGetValue(playerId, out var wh))
        {
            wh = new PlayerWarehouse(playerId);
            _players[playerId] = wh;
        }

        return wh;
    }

    public bool TryGet(Guid playerId, out PlayerWarehouse? warehouse)
    {
        return _players.TryGetValue(playerId, out warehouse);
    }
}

public sealed class PlayerWarehouse
{
    private readonly Dictionary<Guid, Item> _items = new();

    public PlayerWarehouse(Guid playerId)
    {
        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));

        PlayerId = playerId;
    }

    public Guid PlayerId { get; }

    public IReadOnlyDictionary<Guid, Item> Items => _items;

    public void AddItem(Item item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));
        _items[item.Id] = item;
    }

    public bool TryGetItem(Guid itemId, out Item? item)
    {
        return _items.TryGetValue(itemId, out item);
    }

    public bool TryTakeItem(Guid itemId, out Item? item)
    {
        if (_items.TryGetValue(itemId, out item))
        {
            _items.Remove(itemId);
            return true;
        }

        item = null;
        return false;
    }
}