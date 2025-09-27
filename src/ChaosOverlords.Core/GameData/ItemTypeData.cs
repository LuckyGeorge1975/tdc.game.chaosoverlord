namespace ChaosOverlords.Core.GameData;

/// <summary>
/// Metadata for converting item type identifiers into readable labels.
/// </summary>
public sealed record ItemTypeData
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string Description { get; init; } = string.Empty;
}
