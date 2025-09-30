using System.Collections.Generic;
using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.App.Messaging;

/// <summary>
/// Broadcast whenever the command timeline changes.
/// </summary>
/// <param name="Phases">Ordered list of phase snapshots for display.</param>
public sealed record CommandTimelineUpdatedMessage(IReadOnlyList<CommandPhaseSnapshot> Phases);

/// <summary>
/// Immutable snapshot describing a command phase entry.
/// </summary>
/// <param name="Phase">The logical command phase.</param>
/// <param name="State">The current state of the phase in the turn pipeline.</param>
public sealed record CommandPhaseSnapshot(CommandPhase Phase, CommandPhaseState State);
