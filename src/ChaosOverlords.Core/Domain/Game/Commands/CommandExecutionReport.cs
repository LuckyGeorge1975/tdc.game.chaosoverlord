using System;
using System.Collections.Generic;
using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Core.Domain.Game.Commands;

/// <summary>
/// Enumerates the outcome for a single command execution attempt.
/// </summary>
public enum CommandExecutionStatus
{
    Completed,
    Failed,
    Skipped
}

/// <summary>
/// Captures the result of executing a specific command.
/// </summary>
public sealed record CommandExecutionEntry(
    Guid CommandId,
    Guid GangId,
    CommandPhase Phase,
    PlayerCommandKind Kind,
    CommandExecutionStatus Status,
    string Message);

/// <summary>
/// Aggregated results for all commands resolved during the execution phase for a player.
/// </summary>
public sealed record CommandExecutionReport(Guid PlayerId, IReadOnlyList<CommandExecutionEntry> Entries)
{
    public static CommandExecutionReport Empty(Guid playerId) => new(playerId, Array.Empty<CommandExecutionEntry>());
}
