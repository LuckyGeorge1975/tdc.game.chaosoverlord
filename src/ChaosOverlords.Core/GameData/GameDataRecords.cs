namespace ChaosOverlords.Core.GameData;

/// <summary>
/// Immutable representation of a gang entry sourced from the embedded game data.
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

/// <summary>
/// Immutable representation of an item entry sourced from the embedded game data.
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

/// <summary>
/// Immutable representation of a site entry sourced from the embedded game data.
/// </summary>
public sealed record SiteData
{
    public required string Name { get; init; }
    public int Resistance { get; init; }
    public int Support { get; init; }
    public int Tolerance { get; init; }
    public int Cash { get; init; }
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
    public int EquipmentDiscountPercent { get; init; }
    public int EnablesResearchThroughTechLevel { get; init; }
    public int Security { get; init; }
    public string Image { get; init; } = string.Empty;
    public string Thumbnail { get; init; } = string.Empty;
}

/// <summary>
/// Metadata for converting item type identifiers into readable labels.
/// </summary>
public sealed record ItemTypeData
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string Description { get; init; } = string.Empty;
}
