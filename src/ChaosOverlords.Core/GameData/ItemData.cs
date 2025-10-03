namespace ChaosOverlords.Core.GameData;

/// <summary>
///     Immutable representation of an item entry sourced from the embedded game data.
/// </summary>
public sealed record ItemData
{
    public required string Name { get; init; }
    public int Type { get; init; }
    public int ResearchCost { get; init; }
    public int FabricationCost { get; init; }
    public int TechLevel { get; init; }
    public int Combat { get; init; }
    public int Defense { get; init; }
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