using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.App.Messaging;

/// <summary>
///     Broadcast whenever the turn lifecycle state changes.
/// </summary>
/// <param name="TurnNumber">The current turn index starting at 1.</param>
/// <param name="CurrentPhase">The active turn phase.</param>
/// <param name="ActiveCommandPhase">The active command sub-phase, if any.</param>
/// <param name="IsTurnActive">Indicates whether a turn is currently in progress.</param>
public sealed record TurnSummaryChangedMessage(
    int TurnNumber,
    TurnPhase CurrentPhase,
    CommandPhase? ActiveCommandPhase,
    bool IsTurnActive);