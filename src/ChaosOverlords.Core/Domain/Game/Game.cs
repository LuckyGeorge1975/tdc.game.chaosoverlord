using ChaosOverlords.Core.Domain.Players;

namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
/// Aggregates the runtime entities that describe a single campaign instance, keeping them in sync with their
/// immutable data counterparts.
/// </summary>
public sealed class Game
{
    private readonly Dictionary<Guid, IPlayer> _players;
    private readonly Dictionary<Guid, Gang> _gangs;
    private readonly Dictionary<Guid, Item> _items;
    private readonly Dictionary<string, Sector> _sectors;

    public Game(
        IEnumerable<IPlayer> players,
        IEnumerable<Sector> sectors,
        IEnumerable<Gang>? gangs = null,
        IEnumerable<Item>? items = null)
    {
        if (players is null)
        {
            throw new ArgumentNullException(nameof(players));
        }

        if (sectors is null)
        {
            throw new ArgumentNullException(nameof(sectors));
        }

        _players = players.ToDictionary(p => p.Id);
        _sectors = sectors.ToDictionary(s => s.Id);
        _gangs = new Dictionary<Guid, Gang>();
        _items = new Dictionary<Guid, Item>();

        if (items is not null)
        {
            foreach (var item in items)
            {
                _items[item.Id] = item;
            }
        }

        if (gangs is not null)
        {
            foreach (var gang in gangs)
            {
                AddGang(gang);
            }
        }
    }

    /// <summary>
    /// All players participating in the current campaign, keyed by runtime id.
    /// </summary>
    public IReadOnlyDictionary<Guid, IPlayer> Players => _players;

    /// <summary>
    /// Known city sectors forming the map for the campaign.
    /// </summary>
    public IReadOnlyDictionary<string, Sector> Sectors => _sectors;

    /// <summary>
    /// Active gangs currently deployed on the board.
    /// </summary>
    public IReadOnlyDictionary<Guid, Gang> Gangs => _gangs;

    /// <summary>
    /// Items that exist in the campaign world, including unequipped armory stock.
    /// </summary>
    public IReadOnlyDictionary<Guid, Item> Items => _items;

    /// <summary>
    /// Retrieves the player runtime state for a known id.
    /// </summary>
    public IPlayer GetPlayer(Guid playerId) => _players[playerId];

    /// <summary>
    /// Attempts to resolve the player runtime state for optional lookups.
    /// </summary>
    public bool TryGetPlayer(Guid playerId, out IPlayer? player) => _players.TryGetValue(playerId, out player);

    /// <summary>
    /// Retrieves the gang runtime state for a known id.
    /// </summary>
    public Gang GetGang(Guid gangId) => _gangs[gangId];

    /// <summary>
    /// Attempts to resolve a gang; used by systems that operate conditionally.
    /// </summary>
    public bool TryGetGang(Guid gangId, out Gang? gang) => _gangs.TryGetValue(gangId, out gang);

    /// <summary>
    /// Retrieves an item instance by id.
    /// </summary>
    public Item GetItem(Guid itemId) => _items[itemId];

    /// <summary>
    /// Attempts to resolve an item instance by id.
    /// </summary>
    public bool TryGetItem(Guid itemId, out Item? item) => _items.TryGetValue(itemId, out item);

    /// <summary>
    /// Resolves the requested sector.
    /// </summary>
    public Sector GetSector(string sectorId) => _sectors[sectorId];

    /// <summary>
    /// Attempts to resolve a sector without throwing.
    /// </summary>
    public bool TryGetSector(string sectorId, out Sector? sector) => _sectors.TryGetValue(sectorId, out sector);

    /// <summary>
    /// Registers a new player, usually during scenario setup.
    /// </summary>
    public void AddPlayer(IPlayer player)
    {
        player = player ?? throw new ArgumentNullException(nameof(player));
        _players.Add(player.Id, player);
    }

    /// <summary>
    /// Registers a gang into the campaign, wiring up ownership, sector presence, and equipment.
    /// </summary>
    public void AddGang(Gang gang)
    {
        gang = gang ?? throw new ArgumentNullException(nameof(gang));

        _gangs.Add(gang.Id, gang);

        var owner = GetPlayer(gang.OwnerId);
        owner.AssignGang(gang.Id);

        if (!_sectors.TryGetValue(gang.SectorId, out var sector))
        {
            throw new InvalidOperationException($"Sector '{gang.SectorId}' is not registered in the game state.");
        }

        sector.PlaceGang(gang);

        foreach (var item in gang.Items)
        {
            _items[item.Id] = item;
        }
    }

    /// <summary>
    /// Executes a move command, relocating a gang between sectors and updating all cross references.
    /// </summary>
    public void MoveGang(Guid gangId, string targetSectorId)
    {
        if (!_sectors.ContainsKey(targetSectorId))
        {
            throw new InvalidOperationException(
                $"Target sector '{targetSectorId}' is not registered in the game state.");
        }

        var gang = GetGang(gangId);
        var currentSector = GetSector(gang.SectorId);
        currentSector.RemoveGang(gangId);

        var targetSector = GetSector(targetSectorId);
        targetSector.PlaceGang(gang);
        gang.MoveTo(targetSectorId);
    }

    /// <summary>
    /// Removes a gang from the board, unwinding sector occupancy, ownership, and equipment references.
    /// </summary>
    public void RemoveGang(Guid gangId)
    {
        if (!_gangs.Remove(gangId, out var gang))
        {
            return;
        }

        if (_players.TryGetValue(gang.OwnerId, out var owner))
        {
            owner.RemoveGang(gangId);
        }

        if (_sectors.TryGetValue(gang.SectorId, out var sector))
        {
            sector.RemoveGang(gangId);
        }

        foreach (var item in gang.Items)
        {
            _items.Remove(item.Id);
        }
    }

    /// <summary>
    /// Associates an item with a gang so that its bonuses are counted going forward.
    /// </summary>
    public void RegisterItem(Item item, Guid owningGangId)
    {
        item = item ?? throw new ArgumentNullException(nameof(item));

        var gang = GetGang(owningGangId);
        gang.AttachItem(item);
        _items[item.Id] = item;
    }

    /// <summary>
    /// Removes an item from play. If the item is attached to a gang it will also be unequipped.
    /// </summary>
    public bool RemoveItem(Guid itemId)
    {
        if (!_items.Remove(itemId))
        {
            return false;
        }

        foreach (var gang in _gangs.Values)
        {
            if (gang.DetachItem(itemId))
            {
                break;
            }
        }

        return true;
    }
}