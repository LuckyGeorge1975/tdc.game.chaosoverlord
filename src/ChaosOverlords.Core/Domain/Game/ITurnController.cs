using System;
using System.Collections.Generic;

namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
/// Defines the contract for managing turn progression and command sub-phases.
/// </summary>
public interface ITurnController
{
    bool IsTurnActive { get; }

    TurnPhase CurrentPhase { get; }

    CommandPhase? ActiveCommandPhase { get; }

    int TurnNumber { get; }

    IReadOnlyList<CommandPhaseProgress> CommandPhases { get; }

    bool CanStartTurn { get; }

    bool CanAdvancePhase { get; }

    bool CanEndTurn { get; }

    /// <summary>
    /// Raised whenever the turn state changes.
    /// </summary>
    event EventHandler? StateChanged;

    /// <summary>
    /// Raised when <see cref="EndTurn"/> completes successfully.
    /// </summary>
    event EventHandler? TurnCompleted;

    void StartTurn();

    void AdvancePhase();

    void EndTurn();

    /// <summary>
    /// Restores the controller to its initial idle state.
    /// </summary>
    void Reset();
}
