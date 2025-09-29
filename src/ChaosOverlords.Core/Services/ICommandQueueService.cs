using System;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Commands;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Provides a high-level API for manipulating command queues and producing UI-friendly snapshots.
/// </summary>
public interface ICommandQueueService
{
    CommandQueueSnapshot GetQueue(GameState gameState, Guid playerId);

    CommandQueueResult QueueMove(GameState gameState, Guid playerId, Guid gangId, string targetSectorId, int turnNumber);

    CommandQueueResult QueueControl(GameState gameState, Guid playerId, Guid gangId, string sectorId, int turnNumber);

    CommandQueueResult QueueChaos(GameState gameState, Guid playerId, Guid gangId, string sectorId, int turnNumber);

    CommandQueueResult Remove(GameState gameState, Guid playerId, Guid gangId, int turnNumber);
}
