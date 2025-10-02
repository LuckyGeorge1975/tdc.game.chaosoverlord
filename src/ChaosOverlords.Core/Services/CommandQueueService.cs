using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Commands;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Default implementation of <see cref="ICommandQueueService"/> coordinating validation and queue management.
/// </summary>
public sealed class CommandQueueService : ICommandQueueService
{
    private static readonly CommandPhase[] CommandPhaseOrder =
    {
        CommandPhase.Instant,
        CommandPhase.Combat,
        CommandPhase.Transaction,
        CommandPhase.Chaos,
        CommandPhase.Movement,
        CommandPhase.Control
    };

    private static readonly IReadOnlyDictionary<CommandPhase, int> CommandPhaseRank = CommandPhaseOrder
        .Select((phase, index) => (phase, index))
        .ToDictionary(tuple => tuple.phase, tuple => tuple.index);

    public CommandQueueSnapshot GetQueue(GameState gameState, Guid playerId)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        var queue = gameState.Commands.TryGet(playerId, out var existing)
            ? existing
            : null;

        return CreateSnapshot(gameState, queue, playerId);
    }

    public CommandQueueResult QueueMove(GameState gameState, Guid playerId, Guid gangId, string targetSectorId, int turnNumber)
    {
        ValidateArguments(gameState, playerId, gangId, turnNumber);
        if (string.IsNullOrWhiteSpace(targetSectorId))
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidSector, "Target sector id must be provided.");
        }

        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetPlayer(playerId, out var player) || player is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidPlayer, "Player not found.");
        }

        if (!game.TryGetGang(gangId, out var gang) || gang is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidGang, "Gang not found.");
        }

        if (gang.OwnerId != playerId)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.NotOwned, "Gang does not belong to the player.");
        }

        if (!game.TryGetSector(targetSectorId, out var targetSector) || targetSector is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidSector, "Target sector does not exist.");
        }

        if (string.Equals(gang.SectorId, targetSector.Id, StringComparison.OrdinalIgnoreCase))
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidAction, "Gang is already in the target sector.");
        }

        if (!SectorGrid.AreAdjacent(gang.SectorId, targetSector.Id))
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.NotAdjacent, "Target sector is not adjacent to the gang's current position.");
        }

        // Capacity: max 6 of the player's gangs per sector
        var ownerGangCountInTarget = game.Gangs.Values.Count(g => g.OwnerId == playerId && string.Equals(g.SectorId, targetSector.Id, StringComparison.OrdinalIgnoreCase));
        if (ownerGangCountInTarget >= 6)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidAction, "Target sector is full (max 6 of your gangs).");
        }

        var command = new MoveCommand(Guid.NewGuid(), playerId, gangId, turnNumber, gang.SectorId, targetSector.Id);
        return EnqueueCommand(gameState, command, string.Format(CultureInfo.CurrentCulture, "Move to {0} queued.", targetSector.Id));
    }

    public CommandQueueResult QueueControl(GameState gameState, Guid playerId, Guid gangId, string sectorId, int turnNumber)
    {
        ValidateArguments(gameState, playerId, gangId, turnNumber);
        if (string.IsNullOrWhiteSpace(sectorId))
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidSector, "Sector id must be provided.");
        }

        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetPlayer(playerId, out var player) || player is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidPlayer, "Player not found.");
        }

        if (!game.TryGetGang(gangId, out var gang) || gang is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidGang, "Gang not found.");
        }

        if (gang.OwnerId != playerId)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.NotOwned, "Gang does not belong to the player.");
        }

        if (!game.TryGetSector(sectorId, out var sector) || sector is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidSector, "Sector does not exist.");
        }

        if (!string.Equals(gang.SectorId, sector.Id, StringComparison.OrdinalIgnoreCase))
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidAction, "Gang must occupy the sector to attempt control.");
        }

        var command = new ControlCommand(Guid.NewGuid(), playerId, gangId, turnNumber, sector.Id);
        return EnqueueCommand(gameState, command, string.Format(CultureInfo.CurrentCulture, "Control attempt queued for {0}.", sector.Id));
    }

    public CommandQueueResult QueueInfluence(GameState gameState, Guid playerId, Guid gangId, string sectorId, int turnNumber)
    {
        ValidateArguments(gameState, playerId, gangId, turnNumber);
        if (string.IsNullOrWhiteSpace(sectorId))
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidSector, "Sector id must be provided.");
        }

        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetPlayer(playerId, out var player) || player is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidPlayer, "Player not found.");
        }

        if (!game.TryGetGang(gangId, out var gang) || gang is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidGang, "Gang not found.");
        }

        if (gang.OwnerId != playerId)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.NotOwned, "Gang does not belong to the player.");
        }

        if (!game.TryGetSector(sectorId, out var sector) || sector is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidSector, "Sector does not exist.");
        }

        // Validate presence: accept either explicit occupancy or matching current sector id.
        if (!sector.GangIds.Contains(gang.Id) && !string.Equals(gang.SectorId, sector.Id, StringComparison.OrdinalIgnoreCase))
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidAction, "Gang must occupy the sector to influence the site.");
        }

        if (sector.ControllingPlayerId != playerId)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidAction, "Sector must be under your control to influence the site.");
        }

        // Allow queuing even if already influenced; execution-time will skip if no resistance remains.

        var command = new InfluenceCommand(Guid.NewGuid(), playerId, gangId, turnNumber, sector.Id);
        return EnqueueCommand(gameState, command, string.Format(CultureInfo.CurrentCulture, "Influence queued for {0}.", sector.Id));
    }

    public CommandQueueResult QueueChaos(GameState gameState, Guid playerId, Guid gangId, string sectorId, int turnNumber)
    {
        ValidateArguments(gameState, playerId, gangId, turnNumber);
        if (string.IsNullOrWhiteSpace(sectorId))
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidSector, "Sector id must be provided.");
        }

        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetPlayer(playerId, out var player) || player is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidPlayer, "Player not found.");
        }

        if (!game.TryGetGang(gangId, out var gang) || gang is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidGang, "Gang not found.");
        }

        if (gang.OwnerId != playerId)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.NotOwned, "Gang does not belong to the player.");
        }

        if (!game.TryGetSector(sectorId, out var sector) || sector is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidSector, "Sector does not exist.");
        }

        if (!string.Equals(gang.SectorId, sector.Id, StringComparison.OrdinalIgnoreCase))
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidAction, "Gang must occupy the sector to project chaos.");
        }

        var projectedChaos = Math.Max(0, gang.TotalStats.Chaos);
        var command = new ChaosCommand(Guid.NewGuid(), playerId, gangId, turnNumber, sector.Id, projectedChaos);
        return EnqueueCommand(gameState, command, string.Format(CultureInfo.CurrentCulture, "Chaos projection queued for {0}.", sector.Id));
    }

    public CommandQueueResult Remove(GameState gameState, Guid playerId, Guid gangId, int turnNumber)
    {
        ValidateArguments(gameState, playerId, gangId, turnNumber);

        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetPlayer(playerId, out var player) || player is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.InvalidPlayer, "Player not found.");
        }

        var queue = gameState.Commands.TryGet(playerId, out var existing) ? existing : null;
        if (queue is null)
        {
            return Failure(gameState, playerId, CommandQueueRequestStatus.NotFound, "No queued command for gang.");
        }

        var mutation = queue.Remove(gangId);
        var snapshot = CreateSnapshot(gameState, queue, playerId);
        return mutation.Status switch
        {
            CommandQueueMutationStatus.Removed => new CommandQueueResult(CommandQueueRequestStatus.Removed, "Command removed.", snapshot, null, mutation.ReplacedCommand),
            CommandQueueMutationStatus.NotFound => new CommandQueueResult(CommandQueueRequestStatus.NotFound, "No command queued for the specified gang.", snapshot),
            _ => new CommandQueueResult(CommandQueueRequestStatus.NotFound, "No command queued for the specified gang.", snapshot)
        };
    }

    private static void ValidateArguments(GameState gameState, Guid playerId, Guid gangId, int turnNumber)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (playerId == Guid.Empty)
        {
            throw new ArgumentException("Player id must be provided.", nameof(playerId));
        }

        if (gangId == Guid.Empty)
        {
            throw new ArgumentException("Gang id must be provided.", nameof(gangId));
        }

        if (turnNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(turnNumber), turnNumber, "Turn number must be positive.");
        }
    }

    private CommandQueueResult EnqueueCommand(GameState gameState, PlayerCommand command, string successMessage)
    {
        var queue = gameState.Commands.GetOrCreate(command.PlayerId);
        var mutation = queue.SetCommand(command);
        var snapshot = CreateSnapshot(gameState, queue, command.PlayerId);

        var status = mutation.Status switch
        {
            CommandQueueMutationStatus.Added => CommandQueueRequestStatus.Success,
            CommandQueueMutationStatus.Replaced => CommandQueueRequestStatus.Replaced,
            _ => CommandQueueRequestStatus.InvalidAction
        };

        var message = mutation.Status switch
        {
            CommandQueueMutationStatus.Added => successMessage,
            CommandQueueMutationStatus.Replaced => string.Format(CultureInfo.CurrentCulture, "Command updated: {0}", successMessage),
            _ => "Command could not be queued."
        };

        return new CommandQueueResult(status, message, snapshot, mutation.Command, mutation.ReplacedCommand);
    }

    private CommandQueueResult Failure(GameState gameState, Guid playerId, CommandQueueRequestStatus status, string message)
    {
        var snapshot = gameState.Commands.TryGet(playerId, out var queue)
            ? CreateSnapshot(gameState, queue, playerId)
            : new CommandQueueSnapshot(playerId, Array.Empty<PlayerCommandSnapshot>());

        return new CommandQueueResult(status, message, snapshot);
    }

    private CommandQueueSnapshot CreateSnapshot(GameState gameState, CommandQueue? queue, Guid playerId)
    {
        if (gameState.Game is null)
        {
            throw new InvalidOperationException("Game state is missing runtime data.");
        }

        if (queue is null)
        {
            return new CommandQueueSnapshot(playerId, Array.Empty<PlayerCommandSnapshot>());
        }

        var entries = queue.Commands
            .OrderBy(command => CommandPhaseRank.TryGetValue(command.Phase, out var rank) ? rank : int.MaxValue)
            .ThenBy(command => command.TurnNumber)
            .Select(command => CreateEntry(gameState, command))
            .ToList();

        return new CommandQueueSnapshot(queue.PlayerId, entries);
    }

    private PlayerCommandSnapshot CreateEntry(GameState gameState, PlayerCommand command)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        var gang = game.TryGetGang(command.GangId, out var resolvedGang) ? resolvedGang : null;
        var gangName = gang?.Data.Name ?? string.Format(CultureInfo.CurrentCulture, "Gang {0}", command.GangId);

        return command switch
        {
            MoveCommand move => new PlayerCommandSnapshot(
                move.CommandId,
                move.GangId,
                gangName,
                move.Kind,
                move.Phase,
                move.SourceSectorId,
                move.TargetSectorId,
                0,
                move.TurnNumber,
                string.Format(CultureInfo.CurrentCulture, "Move from {0} to {1}", move.SourceSectorId, move.TargetSectorId)),
            InfluenceCommand influence => new PlayerCommandSnapshot(
                influence.CommandId,
                influence.GangId,
                gangName,
                influence.Kind,
                influence.Phase,
                influence.SectorId,
                influence.SectorId,
                0,
                influence.TurnNumber,
                string.Format(CultureInfo.CurrentCulture, "Influence {0}", influence.SectorId)),
            ControlCommand control => new PlayerCommandSnapshot(
                control.CommandId,
                control.GangId,
                gangName,
                control.Kind,
                control.Phase,
                control.SectorId,
                control.SectorId,
                0,
                control.TurnNumber,
                string.Format(CultureInfo.CurrentCulture, "Control {0}", control.SectorId)),
            ChaosCommand chaos => new PlayerCommandSnapshot(
                chaos.CommandId,
                chaos.GangId,
                gangName,
                chaos.Kind,
                chaos.Phase,
                chaos.SectorId,
                chaos.SectorId,
                chaos.ProjectedChaos,
                chaos.TurnNumber,
                string.Format(CultureInfo.CurrentCulture, "Chaos projection {0} in {1}", chaos.ProjectedChaos, chaos.SectorId)),
            _ => new PlayerCommandSnapshot(
                command.CommandId,
                command.GangId,
                gangName,
                command.Kind,
                command.Phase,
                null,
                null,
                0,
                command.TurnNumber,
                null)
        };
    }
}
