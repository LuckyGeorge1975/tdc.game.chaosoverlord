using System;
using System.Collections.Generic;

namespace ChaosOverlords.Core.Domain.Game.Events;

/// <summary>
/// Stores and exposes turn-related events for consumption by the application layer.
/// </summary>
public interface ITurnEventLog
{
    /// <summary>
    /// Raised whenever the log is mutated.
    /// </summary>
    event EventHandler? EventsChanged;

    /// <summary>
    /// Maximum number of events retained in the log.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// All events currently stored in chronological order (oldest first).
    /// </summary>
    IReadOnlyList<TurnEvent> Events { get; }

    /// <summary>
    /// Appends a new event to the log; older entries beyond <see cref="Capacity"/> are dropped.
    /// </summary>
    void Append(TurnEvent entry);

    /// <summary>
    /// Clears all events from the log.
    /// </summary>
    void Clear();
}
