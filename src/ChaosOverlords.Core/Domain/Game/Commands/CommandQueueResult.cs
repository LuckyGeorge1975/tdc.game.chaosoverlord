namespace ChaosOverlords.Core.Domain.Game.Commands;

/// <summary>
///     Enumerates result statuses when interacting with the command queue.
/// </summary>
public enum CommandQueueRequestStatus
{
    Success,
    Replaced,
    Removed,
    NotFound,
    InvalidPlayer,
    InvalidGang,
    NotOwned,
    InvalidSector,
    NotAdjacent,
    InvalidAction
}

/// <summary>
///     Wrapper describing the outcome of a queue operation along with an updated snapshot.
/// </summary>
public sealed record CommandQueueResult(
    CommandQueueRequestStatus Status,
    string Message,
    CommandQueueSnapshot Snapshot,
    PlayerCommand? Command = null,
    PlayerCommand? ReplacedCommand = null);