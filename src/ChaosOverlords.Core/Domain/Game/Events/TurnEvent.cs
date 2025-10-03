namespace ChaosOverlords.Core.Domain.Game.Events;

/// <summary>
///     Represents a single entry in the turn event log.
/// </summary>
public sealed record TurnEvent(
    int TurnNumber,
    TurnPhase Phase,
    CommandPhase? CommandPhase,
    TurnEventType Type,
    string Description,
    DateTimeOffset Timestamp);