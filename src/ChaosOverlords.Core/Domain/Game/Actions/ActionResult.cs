using System.Collections.ObjectModel;
using System.Globalization;

namespace ChaosOverlords.Core.Domain.Game.Actions;

/// <summary>
///     Final outcome of an action resolution attempt.
/// </summary>
public sealed class ActionResult
{
    private static readonly ReadOnlyCollection<ActionModifier> EmptyModifiers = new(Array.Empty<ActionModifier>());
    private readonly ReadOnlyCollection<ActionModifier> _appliedModifiers;

    private ActionResult(
        ActionContext context,
        PercentileRollResult roll,
        int effectiveChance,
        int netModifier,
        ActionCheckOutcome outcome,
        ReadOnlyCollection<ActionModifier> appliedModifiers)
    {
        Context = context;
        Roll = roll;
        EffectiveChance = effectiveChance;
        NetModifier = netModifier;
        Outcome = outcome;
        _appliedModifiers = appliedModifiers;
    }

    /// <summary>
    ///     Original context that described the action.
    /// </summary>
    public ActionContext Context { get; }

    /// <summary>
    ///     Percentile roll used during evaluation.
    /// </summary>
    public PercentileRollResult Roll { get; }

    /// <summary>
    ///     Effective chance (after modifiers) against which the roll was compared.
    /// </summary>
    public int EffectiveChance { get; }

    /// <summary>
    ///     Net sum of all applied modifiers.
    /// </summary>
    public int NetModifier { get; }

    /// <summary>
    ///     Outcome of the action.
    /// </summary>
    public ActionCheckOutcome Outcome { get; }

    /// <summary>
    ///     Applied modifiers for logging/inspection.
    /// </summary>
    public IReadOnlyList<ActionModifier> AppliedModifiers => _appliedModifiers;

    /// <summary>
    ///     Convenience flag exposing whether the action is considered successful.
    /// </summary>
    public bool IsSuccess => Outcome is ActionCheckOutcome.Success or ActionCheckOutcome.AutomaticSuccess;

    /// <summary>
    ///     Creates an action result based on a percentile roll.
    /// </summary>
    public static ActionResult FromRoll(ActionContext context, PercentileRollResult roll,
        ActionCheckOutcome? forcedOutcome = null)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        if (roll is null) throw new ArgumentNullException(nameof(roll));

        var appliedModifiers = context.Modifiers.Count == 0
            ? EmptyModifiers
            : new ReadOnlyCollection<ActionModifier>(context.Modifiers.ToArray());

        var effectiveChance = context.Difficulty.ApplyModifiers(appliedModifiers, out var netModifier);
        var outcome = forcedOutcome ?? DetermineOutcome(context.Difficulty, roll, effectiveChance);

        return new ActionResult(context, roll, effectiveChance, netModifier, outcome, appliedModifiers);
    }

    private static ActionCheckOutcome DetermineOutcome(ActionDifficulty difficulty, PercentileRollResult roll,
        int effectiveChance)
    {
        if (roll.Roll <= difficulty.AutomaticSuccessThreshold) return ActionCheckOutcome.AutomaticSuccess;

        if (roll.Roll >= difficulty.AutomaticFailureThreshold) return ActionCheckOutcome.AutomaticFailure;

        return roll.Roll <= effectiveChance
            ? ActionCheckOutcome.Success
            : ActionCheckOutcome.Failure;
    }

    public override string ToString()
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            "{0} → {1} (Roll {2} vs {3}% | Δ {4})",
            Context.ActionName,
            Outcome,
            Roll.Roll,
            EffectiveChance,
            NetModifier >= 0 ? $"+{NetModifier}" : NetModifier.ToString(CultureInfo.CurrentCulture));
    }
}