using System.Globalization;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Actions;
using ChaosOverlords.Core.Domain.Game.Commands;
using ChaosOverlords.Core.Domain.Game.Events;

namespace ChaosOverlords.Core.Services;

/// <summary>
///     Executes queued player commands in deterministic phase order.
/// </summary>
public sealed class CommandResolutionService(
    ITurnEventWriter eventWriter,
    IRngService rng,
    IResearchService researchService,
    IDataService dataService,
    IEquipmentService equipmentService)
    : ICommandResolutionService
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

    private readonly IDataService _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
    private readonly IEquipmentService _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));

    private readonly ITurnEventWriter _eventWriter = eventWriter ?? throw new ArgumentNullException(nameof(eventWriter));
    private readonly IResearchService _researchService = researchService ?? throw new ArgumentNullException(nameof(researchService));
    private readonly IRngService _rng = rng ?? throw new ArgumentNullException(nameof(rng));

    public CommandResolutionService(ITurnEventWriter eventWriter, IRngService rng, IResearchService researchService)
        : this(eventWriter, rng, researchService, new EmptyDataService(), new EquipmentService())
    {
    }

    public CommandExecutionReport Execute(GameState gameState, Guid playerId, int turnNumber)
    {
        if (gameState is null) throw new ArgumentNullException(nameof(gameState));

        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));

        if (turnNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(turnNumber), turnNumber, "Turn number must be positive.");

        if (gameState.Game is null) throw new InvalidOperationException("Game state is missing runtime data.");

        if (!gameState.Commands.TryGet(playerId, out var queue) || queue is null || queue.Commands.Count == 0)
            return CommandExecutionReport.Empty(playerId);

        var entries = new List<CommandExecutionEntry>();
        foreach (var phase in PhaseOrder)
        foreach (var command in queue.GetCommandsForPhase(phase))
        {
            var entry = ExecuteCommand(gameState, command, turnNumber);
            entries.Add(entry);
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
            InfluenceCommand influence => ExecuteInfluence(gameState, influence, turnNumber),
            ControlCommand control => ExecuteControl(gameState, control, turnNumber),
            ChaosCommand chaos => ExecuteChaos(gameState, chaos, turnNumber),
            ResearchCommand research => ExecuteResearch(gameState, research, turnNumber),
            FabricateCommand fabricate => ExecuteFabricate(gameState, fabricate, turnNumber),
            _ => new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Skipped, "Unsupported command type.")
        };
    }

    private CommandExecutionEntry ExecuteFabricate(GameState gameState, FabricateCommand command, int turnNumber)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetGang(command.GangId, out var gang) || gang is null)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Gang not found.");

        if (!game.TryGetPlayer(command.PlayerId, out var player) || player is null)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Player not found.");

        // Resolve item blueprint
        var items = _dataService.GetItemsAsync().GetAwaiter().GetResult();
        var itemData = items.FirstOrDefault(i => string.Equals(i.Name, command.ItemName, StringComparison.Ordinal));
        if (itemData is null)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, $"Unknown item: {command.ItemName}.");

        // Check research unlock if required
        var requiresResearch = itemData.ResearchCost > 0;
        if (requiresResearch)
        {
            var pr = gameState.Research.GetOrCreate(command.PlayerId);
            if (!pr.IsUnlocked(itemData.Name))
                return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                    CommandExecutionStatus.Failed, $"Item not unlocked by research: {itemData.Name}.");
        }

        // Check cash
        var cost = Math.Max(0, itemData.FabricationCost);
        if (player.Cash < cost)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Insufficient funds for fabrication.");

        // Debit and add to warehouse
        if (cost > 0) player.Debit(cost);
        var item = new Item(Guid.NewGuid(), itemData);
        gameState.Warehouse.GetOrCreate(player.Id).AddItem(item);

        var message = string.Format(CultureInfo.CurrentCulture, "Fabricated {0} for {1} c$, added to warehouse.",
            itemData.Name, cost);
        _eventWriter.Write(turnNumber, TurnPhase.Execution, TurnEventType.Information, message,
            CommandPhase.Transaction);
        return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
            CommandExecutionStatus.Completed, message);
    }

    private CommandExecutionEntry ExecuteResearch(GameState gameState, ResearchCommand command, int turnNumber)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetGang(command.GangId, out var gang) || gang is null)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Gang not found.");

        var result = _researchService.Execute(gameState, command.PlayerId, command.ProjectId, turnNumber);
        var status = result.Status == ResearchActionStatus.Success ? CommandExecutionStatus.Completed
            : result.Status == ResearchActionStatus.Failed ? CommandExecutionStatus.Failed
            : CommandExecutionStatus.Skipped;

        var message = string.IsNullOrWhiteSpace(result.Message)
            ? string.Format(CultureInfo.CurrentCulture, "{0} contributed research to {1} (+{2}).", gang.Data.Name,
                command.ProjectId, result.ProgressApplied)
            : result.Message;

        _eventWriter.Write(turnNumber, TurnPhase.Execution, TurnEventType.Information, message, CommandPhase.Instant);
        return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind, status,
            message);
    }

    private CommandExecutionEntry ExecuteInfluence(GameState gameState, InfluenceCommand command, int turnNumber)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetGang(command.GangId, out var gang) || gang is null)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Gang not found.");

        if (!game.TryGetSector(command.SectorId, out var sector) || sector is null)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Sector not found.");

        // Validate presence using sector occupancy rather than relying on gang.SectorId equality.
        if (!sector.GangIds.Contains(gang.Id))
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Gang is no longer stationed in the sector.");

        if (sector.ControllingPlayerId != command.PlayerId)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Sector not controlled by the player.");

        if (sector.IsInfluenced)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Skipped, "Site already fully influenced.");

        var influencePower = gang.TotalStats.Influence;
        var siteSupport = Math.Max(0, sector.Site?.Support ?? 0);
        var siteSecurity = Math.Max(0, sector.Site?.Security ?? 0);
        var penalty = siteSupport + siteSecurity;
        var netScore = influencePower - penalty;
        var success = netScore >= 1;

        var modifiers = new List<ActionModifier>
        {
            new("Influence", influencePower)
        };
        if (siteSupport > 0) modifiers.Add(new ActionModifier("Support Penalty", -siteSupport));
        if (siteSecurity > 0) modifiers.Add(new ActionModifier("Security Penalty", -siteSecurity));
        modifiers.Add(new ActionModifier("Net Score", netScore));

        var difficulty = new ActionDifficulty(50);
        var targetLabel = sector.Site is null
            ? sector.Id
            : string.Format(CultureInfo.CurrentCulture, "{0} ({1})", sector.Id, sector.Site.Name);

        var actionContext = new ActionContext(
            turnNumber,
            gang.Id,
            gang.Data.Name,
            "Influence Attempt",
            TurnPhase.Execution,
            CommandPhase.Instant,
            difficulty,
            modifiers,
            sector.Id,
            targetLabel);

        var roll = _rng.RollPercent();
        var forcedOutcome = success ? ActionCheckOutcome.AutomaticSuccess : ActionCheckOutcome.AutomaticFailure;
        var actionResult = ActionResult.FromRoll(actionContext, roll, forcedOutcome);
        _eventWriter.WriteAction(actionResult);

        if (success)
        {
            // Each successful Influence reduces resistance by 1 (Phase 2 scope). Clamp at 0.
            sector.ReduceInfluenceResistance(1);
            var message = string.Format(CultureInfo.CurrentCulture, "{0} influenced {1}. Remaining resistance: {2}.",
                gang.Data.Name, sector.Id, sector.InfluenceResistance);
            _eventWriter.Write(turnNumber, TurnPhase.Execution, TurnEventType.Information, message,
                CommandPhase.Instant);
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Completed, message);
        }

        var failureMessage = string.Format(CultureInfo.CurrentCulture, "{0} failed to influence {1}.", gang.Data.Name,
            sector.Id);
        return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
            CommandExecutionStatus.Failed, failureMessage);
    }

    private CommandExecutionEntry ExecuteMove(GameState gameState, MoveCommand command, int turnNumber)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetGang(command.GangId, out var gang) || gang is null)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Gang not found.");

        if (!game.TryGetSector(command.TargetSectorId, out var sector) || sector is null)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Target sector not found.");

        if (!SectorGrid.AreAdjacent(gang.SectorId, sector.Id))
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Target sector is no longer adjacent.");

        if (string.Equals(gang.SectorId, sector.Id, StringComparison.OrdinalIgnoreCase))
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Skipped, "Gang already located in target sector.");

        // Capacity: max 6 of the player's gangs per sector
        var ownerGangCountInTarget = gameState.Game.Gangs.Values.Count(g =>
            g.OwnerId == gang.OwnerId && string.Equals(g.SectorId, sector.Id, StringComparison.OrdinalIgnoreCase));
        if (ownerGangCountInTarget >= 6)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Target sector is full (max 6 of your gangs).");

        game.MoveGang(command.GangId, sector.Id);

        var message = string.Format(CultureInfo.CurrentCulture, "{0} moved to {1}.", gang.Data.Name, sector.Id);
        _eventWriter.Write(turnNumber, TurnPhase.Execution, TurnEventType.Information, message, CommandPhase.Movement);

        return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
            CommandExecutionStatus.Completed, message);
    }

    private CommandExecutionEntry ExecuteControl(GameState gameState, ControlCommand command, int turnNumber)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetGang(command.GangId, out var gang) || gang is null)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Gang not found.");

        if (!game.TryGetSector(command.SectorId, out var sector) || sector is null)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Sector not found.");

        if (!string.Equals(gang.SectorId, sector.Id, StringComparison.OrdinalIgnoreCase))
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Gang is no longer stationed in the sector.");

        var controlPower = gang.TotalStats.Control + gang.TotalStats.Strength;
        var incomePenalty = Math.Max(0, sector.Site?.Cash ?? 0);
        var supportPenalty = Math.Max(0, sector.Site?.Support ?? 0);
        var netScore = controlPower - incomePenalty - supportPenalty;
        var success = netScore >= 1;

        var modifiers = new List<ActionModifier>
        {
            new("Control", gang.TotalStats.Control),
            new("Strength", gang.TotalStats.Strength)
        };

        if (incomePenalty > 0) modifiers.Add(new ActionModifier("Income Penalty", -incomePenalty));

        if (supportPenalty > 0) modifiers.Add(new ActionModifier("Support Penalty", -supportPenalty));

        modifiers.Add(new ActionModifier("Net Score", netScore));

        var difficulty = new ActionDifficulty(50);
        var targetLabel = sector.Site is null
            ? sector.Id
            : string.Format(CultureInfo.CurrentCulture, "{0} ({1})", sector.Id, sector.Site.Name);

        var actionContext = new ActionContext(
            turnNumber,
            gang.Id,
            gang.Data.Name,
            "Control Attempt",
            TurnPhase.Execution,
            CommandPhase.Control,
            difficulty,
            modifiers,
            sector.Id,
            targetLabel);

        var roll = _rng.RollPercent();
        var forcedOutcome = success ? ActionCheckOutcome.AutomaticSuccess : ActionCheckOutcome.AutomaticFailure;
        var actionResult = ActionResult.FromRoll(actionContext, roll, forcedOutcome);
        _eventWriter.WriteAction(actionResult);

        if (success)
        {
            sector.SetController(command.PlayerId);
            var message = string.Format(CultureInfo.CurrentCulture, "{0} secured control of {1}.", gang.Data.Name,
                sector.Id);
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Completed, message);
        }

        var failureMessage = string.Format(CultureInfo.CurrentCulture, "{0} failed to control {1}.", gang.Data.Name,
            sector.Id);
        return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
            CommandExecutionStatus.Failed, failureMessage);
    }

    private CommandExecutionEntry ExecuteChaos(GameState gameState, ChaosCommand command, int turnNumber)
    {
        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetGang(command.GangId, out var gang) || gang is null)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Gang not found.");

        if (!game.TryGetSector(command.SectorId, out var sector) || sector is null)
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Sector not found.");

        if (!string.Equals(gang.SectorId, sector.Id, StringComparison.OrdinalIgnoreCase))
            return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
                CommandExecutionStatus.Failed, "Gang is no longer stationed in the sector.");

        sector.SetChaosProjection(command.ProjectedChaos);
        var message = string.Format(CultureInfo.CurrentCulture, "{0} projected chaos {1} in {2}.", gang.Data.Name,
            command.ProjectedChaos, sector.Id);
        _eventWriter.Write(turnNumber, TurnPhase.Execution, TurnEventType.Information, message, CommandPhase.Chaos);
        return new CommandExecutionEntry(command.CommandId, command.GangId, command.Phase, command.Kind,
            CommandExecutionStatus.Completed, message);
    }
}