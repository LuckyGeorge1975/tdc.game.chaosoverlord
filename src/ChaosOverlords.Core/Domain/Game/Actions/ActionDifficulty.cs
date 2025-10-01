using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ChaosOverlords.Core.Domain.Game.Actions;

/// <summary>
/// Encapsulates the base chance of an action together with clamping rules for modifiers.
/// </summary>
public sealed class ActionDifficulty
{
    public ActionDifficulty(
        int baseChance,
        int minimumChance = 5,
        int maximumChance = 95,
        int automaticSuccessThreshold = 1,
        int automaticFailureThreshold = 100)
    {
        if (minimumChance < 0 || minimumChance > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumChance), minimumChance, "Minimum chance must be between 0 and 100.");
        }

        if (maximumChance < 0 || maximumChance > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumChance), maximumChance, "Maximum chance must be between 0 and 100.");
        }

        if (minimumChance > maximumChance)
        {
            throw new ArgumentException("Minimum chance cannot exceed maximum chance.", nameof(minimumChance));
        }

        if (automaticSuccessThreshold < 1 || automaticSuccessThreshold > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(automaticSuccessThreshold), automaticSuccessThreshold, "Automatic success threshold must lie between 1 and 100.");
        }

        if (automaticFailureThreshold < 1 || automaticFailureThreshold > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(automaticFailureThreshold), automaticFailureThreshold, "Automatic failure threshold must lie between 1 and 100.");
        }

        if (automaticSuccessThreshold >= automaticFailureThreshold)
        {
            throw new ArgumentException("Automatic success threshold must be lower than automatic failure threshold.", nameof(automaticSuccessThreshold));
        }

        BaseChance = baseChance;
        MinimumChance = minimumChance;
        MaximumChance = maximumChance;
        AutomaticSuccessThreshold = automaticSuccessThreshold;
        AutomaticFailureThreshold = automaticFailureThreshold;
    }

    /// <summary>
    /// Base chance before modifiers are applied. Can exceed bounds and will be clamped when modifiers are evaluated.
    /// </summary>
    public int BaseChance { get; }

    /// <summary>
    /// Lower clamp applied after modifiers.
    /// </summary>
    public int MinimumChance { get; }

    /// <summary>
    /// Upper clamp applied after modifiers.
    /// </summary>
    public int MaximumChance { get; }

    /// <summary>
    /// Natural roll value that always succeeds irrespective of modifiers or effective chance.
    /// </summary>
    public int AutomaticSuccessThreshold { get; }

    /// <summary>
    /// Natural roll value that always fails irrespective of modifiers or effective chance.
    /// </summary>
    public int AutomaticFailureThreshold { get; }

    /// <summary>
    /// Applies modifiers and returns the clamped effective chance.
    /// </summary>
    public int ApplyModifiers(IEnumerable<ActionModifier> modifiers, out int netModifier)
    {
        if (modifiers is null)
        {
            throw new ArgumentNullException(nameof(modifiers));
        }

        var modifierList = modifiers as IReadOnlyCollection<ActionModifier> ?? modifiers.ToArray();
        netModifier = modifierList.Sum(m => m.Value);
        var unclamped = BaseChance + netModifier;
        return Math.Clamp(unclamped, MinimumChance, MaximumChance);
    }

    public override string ToString()
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            "Base {0}, Clamp {1}-{2}, Auto {3}/{4}",
            BaseChance,
            MinimumChance,
            MaximumChance,
            AutomaticSuccessThreshold,
            AutomaticFailureThreshold);
    }
}
