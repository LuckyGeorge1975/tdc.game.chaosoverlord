using System;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Actions;
using ChaosOverlords.Core.Domain.Game.Events;

namespace ChaosOverlords.Tests.Domain.Actions;

public sealed class ActionFrameworkTests
{
    [Fact]
    public void ActionDifficulty_ApplyModifiers_ClampsWithinBounds()
    {
        var difficulty = new ActionDifficulty(baseChance: 40, minimumChance: 10, maximumChance: 80);
        var modifiers = new[]
        {
            new ActionModifier("Positive", 100),
            new ActionModifier("Negative", -5)
        };

        var effective = difficulty.ApplyModifiers(modifiers, out var netModifier);

        Assert.Equal(80, effective);
        Assert.Equal(95, netModifier);
    }

    [Fact]
    public void ActionResult_FromRoll_ComputesOutcome()
    {
        var actorId = Guid.NewGuid();
        var difficulty = new ActionDifficulty(baseChance: 60);
        var context = new ActionContext(
            turnNumber: 1,
            actorId: actorId,
            actorName: "Hackers",
            actionName: "Influence",
            phase: TurnPhase.Execution,
            commandPhase: CommandPhase.Control,
            difficulty: difficulty);

        var result = ActionResult.FromRoll(context, new PercentileRollResult(55));

        Assert.Equal(ActionCheckOutcome.Success, result.Outcome);
        Assert.True(result.IsSuccess);
        Assert.Equal(60, result.EffectiveChance);
    }

    [Fact]
    public void ActionResult_FromRoll_RespectsForcedOutcome()
    {
        var actorId = Guid.NewGuid();
        var difficulty = new ActionDifficulty(baseChance: 20);
        var context = new ActionContext(
            turnNumber: 2,
            actorId: actorId,
            actorName: "Bruisers",
            actionName: "Control",
            phase: TurnPhase.Execution,
            commandPhase: CommandPhase.Control,
            difficulty: difficulty);

        var result = ActionResult.FromRoll(context, new PercentileRollResult(10), ActionCheckOutcome.AutomaticFailure);

        Assert.Equal(ActionCheckOutcome.AutomaticFailure, result.Outcome);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void TurnEventWriter_WriteAction_AppendsActionEvent()
    {
        var log = new TurnEventLog();
        var writer = new TurnEventWriter(log);

        var context = new ActionContext(
            turnNumber: 3,
            actorId: Guid.NewGuid(),
            actorName: "Riggers",
            actionName: "Control Attempt",
            phase: TurnPhase.Execution,
            commandPhase: CommandPhase.Control,
            difficulty: new ActionDifficulty(50),
            modifiers: new[] { new ActionModifier("Control", 5) },
            targetId: "C3",
            targetName: "C3 (Night Market)");

        var result = ActionResult.FromRoll(context, new PercentileRollResult(25), ActionCheckOutcome.AutomaticSuccess);

        writer.WriteAction(result);

        var entry = Assert.Single(log.Events);
        Assert.Equal(TurnEventType.Action, entry.Type);
        Assert.Equal(CommandPhase.Control, entry.CommandPhase);
        Assert.Contains("automatic success", entry.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mods:", entry.Description, StringComparison.OrdinalIgnoreCase);
    }
}
