using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Services;

public sealed class EquipmentService : IEquipmentService
{
    // Minimal capacity rule for Task 18: each gang may equip at most 2 items.
    private const int MaxEquippedItemsPerGang = 2;

    public EquipmentActionResult Equip(GameState state, Guid playerId, Guid gangId, Guid itemId, int turnNumber)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));
        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));
        var game = state.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetGang(gangId, out var gang) || gang is null)
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Gang not found.", gangId, itemId, 0);

        var warehouse = state.Warehouse.GetOrCreate(playerId);
        if (!warehouse.TryTakeItem(itemId, out var item) || item is null)
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Item not found in warehouse.", gangId,
                itemId, 0);

        // Inventory capacity check
        if (gang.Items.Count >= MaxEquippedItemsPerGang)
        {
            // Return item since we cannot equip due to capacity
            warehouse.AddItem(item);
            return new EquipmentActionResult(EquipmentActionStatus.Failed,
                "Gang cannot equip more items (capacity reached).", gangId, itemId, 0);
        }

        // TechLevel gate: gang must meet or exceed item tech level
        if (gang.TotalStats.TechLevel < item.TechLevel)
        {
            // Return item to warehouse since equip failed
            warehouse.AddItem(item);
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Gang tech level too low for item.", gangId,
                itemId, 0);
        }

        gang.AttachItem(item);
        return new EquipmentActionResult(EquipmentActionStatus.Success, "Item equipped.", gangId, itemId, 0);
    }

    public EquipmentActionResult Unequip(GameState state, Guid playerId, Guid gangId, Guid itemId, int turnNumber)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));
        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));
        var game = state.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetGang(gangId, out var gang) || gang is null)
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Gang not found.", gangId, itemId, 0);

        if (!gang.ContainsItem(itemId))
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Gang does not have this item equipped.",
                gangId, itemId, 0);

        // Detach and return to warehouse
        // First, capture reference
        var itemRef = gang.Items.First(i => i.Id == itemId);
        var removed = gang.DetachItem(itemId);
        if (!removed)
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Failed to unequip item.", gangId, itemId,
                0);

        // Return to warehouse
        var warehouse = state.Warehouse.GetOrCreate(playerId);
        warehouse.AddItem(itemRef);
        return new EquipmentActionResult(EquipmentActionStatus.Success, "Item unequipped.", gangId, itemId, 0);
    }

    public EquipmentActionResult Give(GameState state, Guid playerId, Guid fromGangId, Guid toGangId, Guid itemId,
        int turnNumber)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));
        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));
        var game = state.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");

        if (!game.TryGetGang(fromGangId, out var fromGang) || fromGang is null)
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Source gang not found.", fromGangId, itemId,
                0);

        if (!game.TryGetGang(toGangId, out var toGang) || toGang is null)
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Target gang not found.", toGangId, itemId,
                0);

        if (fromGang.OwnerId != playerId || toGang.OwnerId != playerId)
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Both gangs must belong to the player.",
                fromGangId, itemId, 0);

        if (!fromGang.ContainsItem(itemId))
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Source gang does not have this item.",
                fromGangId, itemId, 0);

        var item = fromGang.Items.First(i => i.Id == itemId);
        // TechLevel gate on receiver
        if (toGang.TotalStats.TechLevel < item.TechLevel)
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Target gang tech level too low for item.",
                toGangId, itemId, 0);

        // Capacity check on receiver
        if (toGang.Items.Count >= MaxEquippedItemsPerGang)
            return new EquipmentActionResult(EquipmentActionStatus.Failed,
                "Target gang cannot equip more items (capacity reached).", toGangId, itemId, 0);

        fromGang.DetachItem(itemId);
        toGang.AttachItem(item);
        return new EquipmentActionResult(EquipmentActionStatus.Success, "Item given to target gang.", toGangId, itemId,
            0);
    }

    public EquipmentActionResult Sell(GameState state, Guid playerId, Guid gangId, Guid itemId, int turnNumber)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));
        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));
        var game = state.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetPlayer(playerId, out var player) || player is null)
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Player not found.", gangId, itemId, 0);

        if (!game.TryGetGang(gangId, out var gang) || gang is null)
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Gang not found.", gangId, itemId, 0);

        if (!gang.ContainsItem(itemId))
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Gang does not have this item.", gangId,
                itemId, 0);

        var item = gang.Items.First(i => i.Id == itemId);
        var refund = Math.Max(0, item.Data.FabricationCost / 2);

        var removed = gang.DetachItem(itemId);
        if (!removed)
            return new EquipmentActionResult(EquipmentActionStatus.Failed, "Failed to remove item for sale.", gangId,
                itemId, 0);

        player.Credit(refund);
        // Sold items are removed from the game; do not return to warehouse.
        return new EquipmentActionResult(EquipmentActionStatus.Success, $"Item sold for {refund} c$.", gangId, itemId,
            refund);
    }
}