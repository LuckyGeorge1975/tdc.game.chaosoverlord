using ChaosOverlords.Core.Domain.Players;

namespace ChaosOverlords.Core.Domain.Scenario;

/// <summary>
/// Defines the minimal set of knobs required to bootstrap a new campaign for a given scenario.
/// </summary>
public sealed record ScenarioConfig
{
    public required ScenarioType Type { get; init; }

    public required string Name { get; init; }

    /// <summary>
    /// Players that participate in the scenario along with their starting configuration.
    /// The order defines the default turn rotation.
    /// </summary>
    public IReadOnlyList<ScenarioPlayerConfig> Players { get; init; } = Array.Empty<ScenarioPlayerConfig>();

    /// <summary>
    /// Optional predefined map layout. When empty the scenario service will derive the map from player HQ sectors.
    /// </summary>
    public IReadOnlyList<string> MapSectorIds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Optional deterministic seed. When omitted the scenario service will generate one.
    /// </summary>
    public int? Seed { get; init; }
}

/// <summary>
/// Describes the starting point for a player slot in a scenario.
/// </summary>
public sealed record ScenarioPlayerConfig
{
    public required string Name { get; init; }

    public required PlayerKind Kind { get; init; }

    public int StartingCash { get; init; }

    public required string HeadquartersSectorId { get; init; }

    public string? HeadquartersSiteName { get; init; }

    public required string StartingGangName { get; init; }
}
