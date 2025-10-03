using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
///     Runtime representation of an item that is created from immutable reference data and can be equipped by a gang.
/// </summary>
public sealed class Item
{
    public Item(Guid id, ItemData data, bool isEquipped = false)
    {
        Id = id;
        Data = data ?? throw new ArgumentNullException(nameof(data));
        IsEquipped = isEquipped;
    }

    public Guid Id { get; }

    /// <summary>
    ///     Immutable stats and metadata describing the item blueprint.
    /// </summary>
    public ItemData Data { get; }

    public bool IsEquipped { get; private set; }

    public DateTimeOffset AcquiredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Convenience accessor for displaying the item name without re-reading the data object.
    /// </summary>
    public string Name => Data.Name;

    /// <summary>
    ///     Returns the full stat modifiers baked into the underlying reference data.
    /// </summary>
    public StatSheet Modifiers => StatSheet.From(Data);

    /// <summary>
    ///     Resolves the modifiers that currently affect the owning gang (only when equipped).
    /// </summary>
    public StatSheet ActiveBonuses => IsEquipped ? Modifiers : StatSheet.Zero;

    /// <summary>
    ///     Indicates whether this item requires research before it can be produced or used.
    ///     Backed by <see cref="GameData.ItemData.ResearchCost" />.
    /// </summary>
    public bool RequiresResearch => Data.ResearchCost > 0;

    /// <summary>
    ///     The total research points required to unlock this item.
    /// </summary>
    public int ResearchCost => Data.ResearchCost;

    /// <summary>
    ///     The technology level associated with this item blueprint.
    /// </summary>
    public int TechLevel => Data.TechLevel;

    public void Equip()
    {
        IsEquipped = true;
    }

    public void UnEquip()
    {
        IsEquipped = false;
    }
}