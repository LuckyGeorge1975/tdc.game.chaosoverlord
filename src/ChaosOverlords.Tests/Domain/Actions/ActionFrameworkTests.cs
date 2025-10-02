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

    [Fact]
    public void ActionDifficulty_AutomaticSuccessAndFailure_ThresholdsApply()
    {
        var difficulty = new ActionDifficulty(baseChance: 1, minimumChance: 1, maximumChance: 99, automaticSuccessThreshold: 3, automaticFailureThreshold: 98);

        var context = new ActionContext(
            turnNumber: 1,
            actorId: Guid.NewGuid(),
            actorName: "Testers",
            actionName: "Check",
            phase: TurnPhase.Execution,
            commandPhase: CommandPhase.Instant,
            difficulty: difficulty);

        // Natural low roll is automatic success despite tiny effective chance
        var autoSuccess = ActionResult.FromRoll(context, new PercentileRollResult(2));
        Assert.Equal(ActionCheckOutcome.AutomaticSuccess, autoSuccess.Outcome);

        // Natural high roll is automatic failure despite high effective chance
        var contextHigh = new ActionContext(
            turnNumber: 1,
            actorId: Guid.NewGuid(),
            actorName: "Testers",
            actionName: "Check",
            phase: TurnPhase.Execution,
            commandPhase: CommandPhase.Instant,
            difficulty: new ActionDifficulty(baseChance: 90, minimumChance: 1, maximumChance: 99, automaticSuccessThreshold: 3, automaticFailureThreshold: 98));

        var autoFailure = ActionResult.FromRoll(contextHigh, new PercentileRollResult(99));
        Assert.Equal(ActionCheckOutcome.AutomaticFailure, autoFailure.Outcome);
    }

    [Fact]
    public void ActionDifficulty_EmptyModifiers_YieldsBaseChanceClamped()
    {
        var difficulty = new ActionDifficulty(baseChance: 120, minimumChance: 5, maximumChance: 95);
        var effective = difficulty.ApplyModifiers(Array.Empty<ActionModifier>(), out var net);
        Assert.Equal(95, effective);
        Assert.Equal(0, net);
    }

    [Fact]
    public void ActionResult_FromRoll_AppliesModifierAggregation()
    {
        var difficulty = new ActionDifficulty(baseChance: 50);
        var context = new ActionContext(
            turnNumber: 1,
            actorId: Guid.NewGuid(),
            actorName: "Synths",
            actionName: "Influence",
            phase: TurnPhase.Execution,
            commandPhase: CommandPhase.Instant,
            difficulty: difficulty,
            modifiers: new[]
            {
                new ActionModifier("A", 5),
                new ActionModifier("B", -3),
                new ActionModifier("C", 2)
            });

        var result = ActionResult.FromRoll(context, new PercentileRollResult(55));
        Assert.Equal(54, result.EffectiveChance); // 50 + (5 - 3 + 2)
        Assert.Equal(4, result.NetModifier);
    }
}
