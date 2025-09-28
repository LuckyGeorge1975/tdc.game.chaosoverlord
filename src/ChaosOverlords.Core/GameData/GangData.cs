namespace ChaosOverlords.Core.GameData;

/// <summary>
/// Raw data payload for a gang as it appears in the shipped asset files before gameplay rules are applied.
/// </summary>
public sealed record GangData
{
    public required string Name { get; init; }
    public int HiringCost { get; init; }
    public int UpkeepCost { get; init; }
    public int Combat { get; init; }
    public int Defense { get; init; }
    public int TechLevel { get; init; }
    public int Stealth { get; init; }
    public int Detect { get; init; }
    public int Chaos { get; init; }
    public int Control { get; init; }
    public int Heal { get; init; }
    public int Influence { get; init; }
    public int Research { get; init; }
    public int Strength { get; init; }
    public int BladeMelee { get; init; }
    public int Ranged { get; init; }
    public int Fighting { get; init; }
    public int MartialArts { get; init; }
    public string Image { get; init; } = string.Empty;
    public string Thumbnail { get; init; } = string.Empty;
}
