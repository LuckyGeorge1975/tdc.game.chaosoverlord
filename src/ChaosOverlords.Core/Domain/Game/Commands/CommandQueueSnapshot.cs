namespace ChaosOverlords.Core.Domain.Game.Commands;

/// <summary>
///     Immutable DTO representing the queued commands for a single player.
/// </summary>
public sealed record CommandQueueSnapshot(Guid PlayerId, IReadOnlyList<PlayerCommandSnapshot> Commands);

/// <summary>
///     Describes an individual command in a form that is convenient for UI bindings.
/// </summary>
public sealed record PlayerCommandSnapshot(
    Guid CommandId,
    Guid GangId,
    string GangName,
    PlayerCommandKind Kind,
    CommandPhase Phase,
    string? SourceSectorId,
    string? TargetSectorId,
    int ProjectedChaos,
    int TurnNumber,
    string? Description);