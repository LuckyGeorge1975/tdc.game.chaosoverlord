using System.Collections.ObjectModel;
using System.Globalization;

namespace ChaosOverlords.Core.Domain.Game.Actions;

/// <summary>
///     Captures the result of rolling a set of dice.
/// </summary>
public class DiceRollResult
{
    private readonly ReadOnlyCollection<int> _dice;

    public DiceRollResult(ReadOnlyCollection<int> dice, int modifier, string expression)
    {
        _dice = dice ?? throw new ArgumentNullException(nameof(dice));

        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression must be provided.", nameof(expression));

        Modifier = modifier;
        Expression = expression;
    }

    /// <summary>
    ///     Individual dice results in the order they were rolled.
    /// </summary>
    public IReadOnlyList<int> Dice => _dice;

    /// <summary>
    ///     Flat modifier applied to the sum of all dice.
    /// </summary>
    public int Modifier { get; }

    /// <summary>
    ///     Canonical expression describing the roll (e.g. "2d6+1").
    /// </summary>
    public string Expression { get; }

    /// <summary>
    ///     Aggregated total after applying the modifier.
    /// </summary>
    public int Total => _dice.Sum() + Modifier;

    public override string ToString()
    {
        var diceText = string.Join(", ", _dice.Select(d => d.ToString(CultureInfo.InvariantCulture)));
        return string.Format(
            CultureInfo.CurrentCulture,
            "{0} = {1}{2}",
            Expression,
            diceText,
            Modifier != 0
                ? string.Format(CultureInfo.CurrentCulture, " {0}{1}", Modifier > 0 ? "+" : string.Empty, Modifier)
                : string.Empty);
    }
}

/// <summary>
///     Represents a percentile (1d100) roll.
/// </summary>
public sealed class PercentileRollResult : DiceRollResult
{
    public PercentileRollResult(int roll)
        : base(new ReadOnlyCollection<int>(new[] { ValidateRoll(roll) }), 0, "1d100")
    {
    }

    /// <summary>
    ///     Value rolled on the percentile die (1-100 inclusive).
    /// </summary>
    public int Roll => Dice[0];

    private static int ValidateRoll(int value)
    {
        if (value < 1 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(value), value,
                "Percentile roll must be between 1 and 100 inclusive.");

        return value;
    }
}