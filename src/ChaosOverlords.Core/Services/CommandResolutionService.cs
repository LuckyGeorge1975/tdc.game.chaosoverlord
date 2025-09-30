using System;
using System.Collections.Generic;
using System.Globalization;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Commands;
using ChaosOverlords.Core.Domain.Game.Events;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Executes queued player commands in deterministic phase order.
/// </summary>
public sealed class CommandResolutionService : ICommandResolutionService
{
    private static readonly CommandPhase[] PhaseOrder =
    {
        CommandPhase.Instant,
        CommandPhase.Combat,
        CommandPhase.Transaction,
        CommandPhase.Chaos,
        CommandPhase.Movement,
        CommandPhase.Control
    };

    private readonly ITurnEventWriter _eventWriter;

    public CommandResolutionService(ITurnEventWriter eventWriter)
    {
        _eventWriter = eventWriter ?? throw new ArgumentNullException(nameof(eventWriter));
    }

    public CommandExecutionReport Execute(GameState gameState, Guid playerId, int turnNumber)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (playerId == Guid.Empty)
        {
            throw new ArgumentException("Player id must be provided.", nameof(playerId));
        }

        if (turnNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(turnNumber), turnNumber, "Turn number must be positive.");
        }

        if (gameState.Game is null)
        {
            throw new InvalidOperationException("Game state is missing runtime data.");
        }

        if (!gameState.Commands.TryGet(playerId, out var queue) || queue is null || queue.Commands.Count == 0)
        {
            return CommandExecutionReport.Empty(playerId);
        }

        var entries = new List<CommandExecutionEntry>();
        foreach (var phase in PhaseOrder)
        {
            foreach (var command in queue.GetCommandsForPhase(phase))
            {
                var entry = ExecuteCommand(gameState, command, turnNumber);
                entries.Add(entry);
            }
        }

        queue.Clear();
        gameState.Commands.Clear(playerId);

        return new CommandExecutionReport(playerId, entries);
    }

    private CommandExecutionEntry ExecuteCommand(GameState gameState, PlayerCommand command, int turnNumber)
    {
        return command switch
        {
            MoveCommand move => ExecuteMove(gameState, move, turnNumber),
            ControlCommand control => ExecuteControl(gameState, control, turnNumber),
            ChaosCommand chaos => ExecuteChaos(gameState, chaos, turnNumber),
            _ => new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Skipped, "Unsupported command type.")
        };
    }

    private CommandExecutionEntry ExecuteMove(GameState gameState, MoveCommand command, int turnNumber)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetGang(command.GangId, out var gang) || gang is null)
        {
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Failed, "Gang not found.");
        }

        if (!game.TryGetSector(command.TargetSectorId, out var sector) || sector is null)
        {
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Failed, "Target sector not found.");
        }

        if (!SectorGrid.AreAdjacent(gang.SectorId, sector.Id))
        {
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Failed, "Target sector is no longer adjacent.");
        }

        if (string.Equals(gang.SectorId, sector.Id, StringComparison.OrdinalIgnoreCase))
        {
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Skipped, "Gang already located in target sector.");
        }

        game.MoveGang(command.GangId, sector.Id);

        var message = string.Format(CultureInfo.CurrentCulture, "{0} moved to {1}.", gang.Data.Name, sector.Id);
        _eventWriter.Write(turnNumber, TurnPhase.Execution, TurnEventType.Information, message, CommandPhase.Movement);

        return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Completed, message);
    }

    private CommandExecutionEntry ExecuteControl(GameState gameState, ControlCommand command, int turnNumber)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetGang(command.GangId, out var gang) || gang is null)
        {
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Failed, "Gang not found.");
        }

        if (!game.TryGetSector(command.SectorId, out var sector) || sector is null)
        {
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Failed, "Sector not found.");
        }

        if (!string.Equals(gang.SectorId, sector.Id, StringComparison.OrdinalIgnoreCase))
        {
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Failed, "Gang is no longer stationed in the sector.");
        }

        var controlPower = gang.TotalStats.Control + gang.TotalStats.Strength;
        var incomePenalty = Math.Max(0, sector.Site?.Cash ?? 0);
        var supportPenalty = Math.Max(0, sector.Site?.Support ?? 0);
        var netScore = controlPower - incomePenalty - supportPenalty;
        var success = netScore >= 1;

        if (success)
        {
            sector.SetController(command.PlayerId);
            var message = string.Format(CultureInfo.CurrentCulture, "{0} secured control of {1}.", gang.Data.Name, sector.Id);
            _eventWriter.Write(turnNumber, TurnPhase.Execution, TurnEventType.Information, message, CommandPhase.Control);
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Completed, message);
        }

        var failureMessage = string.Format(CultureInfo.CurrentCulture, "{0} failed to control {1}.", gang.Data.Name, sector.Id);
        _eventWriter.Write(turnNumber, TurnPhase.Execution, TurnEventType.Information, failureMessage, CommandPhase.Control);
        return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Failed, failureMessage);
    }

    private CommandExecutionEntry ExecuteChaos(GameState gameState, ChaosCommand command, int turnNumber)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetGang(command.GangId, out var gang) || gang is null)
        {
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Failed, "Gang not found.");
        }

        if (!game.TryGetSector(command.SectorId, out var sector) || sector is null)
        {
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Failed, "Sector not found.");
        }

        if (!string.Equals(gang.SectorId, sector.Id, StringComparison.OrdinalIgnoreCase))
        {
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Failed, "Gang is no longer stationed in the sector.");
        }

        sector.SetChaosProjection(command.ProjectedChaos);
        var message = string.Format(CultureInfo.CurrentCulture, "{0} projected chaos {1} in {2}.", gang.Data.Name, command.ProjectedChaos, sector.Id);
        _eventWriter.Write(turnNumber, TurnPhase.Execution, TurnEventType.Information, message, CommandPhase.Chaos);
        return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, CommandExecutionStatus.Completed, message);
    }
}
