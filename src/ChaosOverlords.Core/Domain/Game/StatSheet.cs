using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
/// Represents a full set of gang-relevant stats that can be combined from different sources (base data, level perks, items).
/// </summary>
public readonly record struct StatSheet(
    int Combat,
    int Defense,
    int TechLevel,
    int Stealth,
    int Detect,
    int Chaos,
    int Control,
    int Heal,
    int Influence,
    int Research,
    int Strength,
    int BladeMelee,
    int Ranged,
    int Fighting,
    int MartialArts)
{
    public static StatSheet Zero { get; } = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    public static StatSheet From(GangData data) => new(
        data.Combat,
        data.Defense,
        data.TechLevel,
        data.Stealth,
        data.Detect,
        data.Chaos,
        data.Control,
        data.Heal,
        data.Influence,
        data.Research,
        data.Strength,
        data.BladeMelee,
        data.Ranged,
        data.Fighting,
        data.MartialArts);

    public static StatSheet From(ItemData data) => new(
        data.Combat,
        data.Defense,
        data.TechLevel,
        data.Stealth,
        data.Detect,
        data.Chaos,
        data.Control,
        data.Heal,
        data.Influence,
        data.Research,
        data.Strength,
        data.BladeMelee,
        data.Ranged,
        data.Fighting,
        data.MartialArts);

    public static StatSheet operator +(StatSheet left, StatSheet right) => new(
        left.Combat + right.Combat,
        left.Defense + right.Defense,
        left.TechLevel + right.TechLevel,
        left.Stealth + right.Stealth,
        left.Detect + right.Detect,
        left.Chaos + right.Chaos,
        left.Control + right.Control,
        left.Heal + right.Heal,
        left.Influence + right.Influence,
        left.Research + right.Research,
        left.Strength + right.Strength,
        left.BladeMelee + right.BladeMelee,
        left.Ranged + right.Ranged,
        left.Fighting + right.Fighting,
        left.MartialArts + right.MartialArts);
}

/// <summary>
/// Identifies an individual stat that can be extracted from a <see cref="StatSheet"/>.
/// </summary>
public enum GangStat
{
    Combat,
    Defense,
    TechLevel,
    Stealth,
    Detect,
    Chaos,
    Control,
    Heal,
    Influence,
    Research,
    Strength,
    BladeMelee,
    Ranged,
    Fighting,
    MartialArts
}

/// <summary>
/// Captures the base, level, and item contributions for a single stat.
/// </summary>
public readonly record struct StatBreakdown(int Base, int Level, int Items)
{
    public int Total => Base + Level + Items;
}
