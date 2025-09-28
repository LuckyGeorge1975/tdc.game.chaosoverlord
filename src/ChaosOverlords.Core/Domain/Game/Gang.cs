using System.Collections.ObjectModel;
using System.Linq;
using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
/// Runtime gang instance created from immutable <see cref="GangData"/>. Holds stateful aspects like ownership and
/// attached equipment while exposing the current stat totals for UI and rule processing.
/// </summary>
public sealed class Gang
{
    private readonly List<Item> _items;
    private StatSheet _levelBonuses;

    public Gang(
        Guid id,
        GangData data,
        Guid ownerId,
        string sectorId,
        IEnumerable<Item>? items = null,
        StatSheet? levelBonuses = null)
    {
        if (string.IsNullOrWhiteSpace(sectorId))
        {
            throw new ArgumentException("Sector id cannot be null or whitespace.", nameof(sectorId));
        }

        Id = id;
        Data = data ?? throw new ArgumentNullException(nameof(data));
        OwnerId = ownerId;
        SectorId = sectorId;
        _levelBonuses = levelBonuses ?? StatSheet.Zero;
        _items = items?.Select(AttachExistingItem).ToList() ?? new List<Item>();
    }

    public Guid Id { get; }

    /// <summary>
    /// Immutable base stats and visuals backing this gang.
    /// </summary>
    public GangData Data { get; }

    public Guid OwnerId { get; }

    public string SectorId { get; private set; }

    /// <summary>
    /// Current strength including base data, level perks, and equipped item bonuses.
    /// </summary>
    public int Strength => StrengthBreakdown.Total;

    public int BaseStrength => BaseStats.Strength;

    public IReadOnlyCollection<Item> Items => new ReadOnlyCollection<Item>(_items);

    /// <summary>
    /// Base stat line sourced directly from <see cref="Data"/>.
    /// </summary>
    public StatSheet BaseStats => StatSheet.From(Data);

    /// <summary>
    /// Aggregate bonuses unlocked through levelling or other permanent upgrades.
    /// </summary>
    public StatSheet LevelBonuses => _levelBonuses;

    /// <summary>
    /// Combined modifiers granted by currently equipped items.
    /// </summary>
    public StatSheet ItemBonuses => _items.Aggregate(StatSheet.Zero, (total, item) => total + item.ActiveBonuses);

    /// <summary>
    /// Full stat line after applying base, level, and item contributions.
    /// </summary>
    public StatSheet TotalStats => BaseStats + LevelBonuses + ItemBonuses;

    /// <summary>
    /// Provides the detailed composition of the strength stat for UI and rules that need the individual parts.
    /// </summary>
    public StatBreakdown StrengthBreakdown => GetBreakdown(GangStat.Strength);

    /// <summary>
    /// Relocates the gang to a different sector, mirroring the player's movement command.
    /// </summary>
    public void MoveTo(string sectorId)
    {
        if (string.IsNullOrWhiteSpace(sectorId))
        {
            throw new ArgumentException("Sector id cannot be null or whitespace.", nameof(sectorId));
        }

        SectorId = sectorId;
    }

    /// <summary>
    /// Applies a raw strength delta, treated as part of the cumulative level bonus (e.g. training, mission rewards).
    /// </summary>
    public void AdjustStrength(int delta)
    {
        if (delta == 0)
        {
            return;
        }

        _levelBonuses = _levelBonuses with { Strength = _levelBonuses.Strength + delta };
    }

    /// <summary>
    /// Attaches an item to the gang, automatically equipping it so its bonuses apply in combat resolution.
    /// </summary>
    public Item AttachItem(Item item)
    {
        item = item ?? throw new ArgumentNullException(nameof(item));
        if (!_items.Contains(item))
        {
            item.Equip();
            _items.Add(item);
        }
        return item;
    }

    /// <summary>
    /// Removes an item from the gang and disables its bonuses.
    /// </summary>
    public bool DetachItem(Guid itemId)
    {
        var index = _items.FindIndex(i => i.Id == itemId);
        if (index < 0)
        {
            return false;
        }

        _items[index].Unequip();
        _items.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Checks whether the gang currently wields the specified item.
    /// </summary>
    public bool ContainsItem(Guid itemId) => _items.Any(i => i.Id == itemId);

    /// <summary>
    /// Applies a multi-stat bonus, typically granted when the gang levels up.
    /// </summary>
    public void ApplyLevelBonus(StatSheet bonus) => _levelBonuses += bonus;

    /// <summary>
    /// Returns the detailed breakdown for the requested stat.
    /// </summary>
    public StatBreakdown GetBreakdown(GangStat stat) => stat switch
    {
        GangStat.Combat => BuildBreakdown(static s => s.Combat),
        GangStat.Defense => BuildBreakdown(static s => s.Defense),
        GangStat.TechLevel => BuildBreakdown(static s => s.TechLevel),
        GangStat.Stealth => BuildBreakdown(static s => s.Stealth),
        GangStat.Detect => BuildBreakdown(static s => s.Detect),
        GangStat.Chaos => BuildBreakdown(static s => s.Chaos),
        GangStat.Control => BuildBreakdown(static s => s.Control),
        GangStat.Heal => BuildBreakdown(static s => s.Heal),
        GangStat.Influence => BuildBreakdown(static s => s.Influence),
        GangStat.Research => BuildBreakdown(static s => s.Research),
        GangStat.Strength => BuildBreakdown(static s => s.Strength),
        GangStat.BladeMelee => BuildBreakdown(static s => s.BladeMelee),
        GangStat.Ranged => BuildBreakdown(static s => s.Ranged),
        GangStat.Fighting => BuildBreakdown(static s => s.Fighting),
        GangStat.MartialArts => BuildBreakdown(static s => s.MartialArts),
        _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, "Unsupported stat requested."),
    };

    private Item AttachExistingItem(Item item)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        item.Equip();
        return item;
    }

    private StatBreakdown BuildBreakdown(Func<StatSheet, int> selector)
    {
        var fromBase = selector(BaseStats);
        var fromLevel = selector(LevelBonuses);
        var fromItems = selector(ItemBonuses);
        return new StatBreakdown(fromBase, fromLevel, fromItems);
    }
}
