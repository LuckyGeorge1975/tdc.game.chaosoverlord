using System;
using System.Collections.Generic;

namespace ChaosOverlords.Core.Domain.Game.Commands;

/// <summary>
/// Aggregates per-player command queues for the active game state.
/// </summary>
public sealed class CommandQueueState
{
    private readonly Dictionary<Guid, CommandQueue> _queues = new();

    public IReadOnlyDictionary<Guid, CommandQueue> Queues => _queues;

    public CommandQueue GetOrCreate(Guid playerId)
    {
        if (!_queues.TryGetValue(playerId, out var queue))
        {
            queue = new CommandQueue(playerId);
            _queues[playerId] = queue;
        }

        return queue;
    }

    public bool TryGet(Guid playerId, out CommandQueue? queue) => _queues.TryGetValue(playerId, out queue);

    public void Clear() => _queues.Clear();

    public void Clear(Guid playerId)
    {
        if (_queues.TryGetValue(playerId, out var queue))
        {
            queue.Clear();
            _queues.Remove(playerId);
        }
    }
}
