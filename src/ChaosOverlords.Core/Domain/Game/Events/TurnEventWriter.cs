using System;
using System.Globalization;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Actions;
using ChaosOverlords.Core.Domain.Game.Economy;

namespace ChaosOverlords.Core.Domain.Game.Events;

/// <summary>
/// Default implementation that materialises <see cref="TurnEvent"/> instances and appends them to the log.
/// </summary>
public sealed class TurnEventWriter : ITurnEventWriter
{
    private readonly ITurnEventLog _eventLog;

    public TurnEventWriter(ITurnEventLog eventLog)
    {
        _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
    }

    public void Write(int turnNumber, TurnPhase phase, TurnEventType type, string description, CommandPhase? commandPhase = null)
    {
        if (turnNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(turnNumber), turnNumber, "Turn number must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description must be provided.", nameof(description));
        }

        var entry = new TurnEvent(turnNumber, phase, commandPhase, type, description, DateTimeOffset.UtcNow);
        _eventLog.Append(entry);
    }

    public void WriteEconomy(int turnNumber, TurnPhase phase, PlayerEconomySnapshot snapshot)
    {
        if (snapshot is null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        var description = FormatEconomyDescription(snapshot);
        Write(turnNumber, phase, TurnEventType.Economy, description);
    }

    public void WriteAction(ActionResult result)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        var context = result.Context;
        var description = FormatActionDescription(result);
        Write(context.TurnNumber, context.Phase, TurnEventType.Action, description, context.CommandPhase);
    }

    private static string FormatEconomyDescription(PlayerEconomySnapshot snapshot)
    {
        var upkeepText = snapshot.Upkeep > 0 ? $"Upkeep {FormatSigned(-snapshot.Upkeep)}" : "Upkeep 0";
        var sectorText = $"Sectors {FormatSigned(snapshot.SectorIncome)}";
        var siteText = $"Sites {FormatSigned(snapshot.SiteIncome)}";
        var netText = $"Net {FormatSigned(snapshot.NetChange)}";
        var balanceText = $"Cash {snapshot.StartingCash.ToString(CultureInfo.InvariantCulture)} → {snapshot.EndingCash.ToString(CultureInfo.InvariantCulture)}";

        return string.Format(
            CultureInfo.CurrentCulture,
            "{0}: {1}, {2}, {3} ⇒ {4} ({5})",
            snapshot.PlayerName,
            upkeepText,
            sectorText,
            siteText,
            netText,
            balanceText);
    }

    private static string FormatActionDescription(ActionResult result)
    {
        var context = result.Context;
        var targetText = string.IsNullOrWhiteSpace(context.TargetName)
            ? string.Empty
            : string.Format(CultureInfo.CurrentCulture, " targeting {0}", context.TargetName);

        var outcomeText = result.Outcome switch
        {
            ActionCheckOutcome.AutomaticSuccess => "automatic success",
            ActionCheckOutcome.AutomaticFailure => "automatic failure",
            ActionCheckOutcome.Success => "success",
            _ => "failure"
        };

        var modifierText = result.AppliedModifiers.Count == 0
            ? "mods: none"
            : string.Format(
                CultureInfo.CurrentCulture,
                "mods: {0}",
                string.Join(
                    ", ",
                    result.AppliedModifiers.Select(m => string.Format(CultureInfo.CurrentCulture, "{0} {1}", m.Name, FormatSigned(m.Value)))));

        return string.Format(
            CultureInfo.CurrentCulture,
            "{0} performed {1}{2}: {3} ({4} {5} vs {6}%) [{7}]",
            context.ActorName,
            context.ActionName,
            targetText,
            outcomeText,
            result.Roll.Expression,
            result.Roll.Roll.ToString(CultureInfo.CurrentCulture),
            result.EffectiveChance.ToString(CultureInfo.CurrentCulture),
            modifierText);
    }

    private static string FormatSigned(int value)
    {
        return value > 0
            ? $"+{value.ToString(CultureInfo.InvariantCulture)}"
            : value.ToString(CultureInfo.InvariantCulture);
    }
}
