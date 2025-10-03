namespace ChaosOverlords.Core.Domain.Game.Actions;

/// <summary>
///     Represents a single modifier applied to an action resolution.
/// </summary>
public sealed class ActionModifier
{
    public ActionModifier(string name, int value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Modifier name must be provided.", nameof(name));

        Name = name;
        Value = value;
    }

    /// <summary>
    ///     Human readable modifier name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Signed modifier value that contributes to the final action chance.
    /// </summary>
    public int Value { get; }

    public override string ToString()
    {
        return $"{Name} {(Value >= 0 ? "+" : string.Empty)}{Value}";
    }
}