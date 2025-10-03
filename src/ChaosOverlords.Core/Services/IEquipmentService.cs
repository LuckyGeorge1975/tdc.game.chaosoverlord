using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Services;

/// <summary>
///     Equipment service managing equipping, unequipping, giving and selling items.
///     This is a placeholder contract to be implemented in Phase 4.
/// </summary>
public interface IEquipmentService
{
    EquipmentActionResult Equip(GameState state, Guid playerId, Guid gangId, Guid itemId, int turnNumber);
    EquipmentActionResult Unequip(GameState state, Guid playerId, Guid gangId, Guid itemId, int turnNumber);

    EquipmentActionResult Give(GameState state, Guid playerId, Guid fromGangId, Guid toGangId, Guid itemId,
        int turnNumber);

    EquipmentActionResult Sell(GameState state, Guid playerId, Guid gangId, Guid itemId, int turnNumber);
}

public enum EquipmentActionStatus
{
    NotImplemented = 0,
    Success = 1,
    Failed = 2
}

public sealed record EquipmentActionResult(
    EquipmentActionStatus Status,
    string Message,
    Guid? GangId,
    Guid? ItemId,
    int CashDelta);