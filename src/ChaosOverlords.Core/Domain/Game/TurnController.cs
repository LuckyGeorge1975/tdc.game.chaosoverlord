using System;
using System.Collections.Generic;
using System.Linq;

namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
/// Default implementation of <see cref="ITurnController"/> that encapsulates
/// the turn phase state machine and command sub-phase timeline.
/// </summary>
public sealed class TurnController : ITurnController
{
    private static readonly TurnPhase[] PhaseOrder =
    {
        TurnPhase.Upkeep,
        TurnPhase.Command,
        TurnPhase.Execution,
        TurnPhase.Hire,
        TurnPhase.Elimination
    };

    private static readonly CommandPhase[] CommandPhaseOrder =
    {
        CommandPhase.Instant,
        CommandPhase.Combat,
        CommandPhase.Transaction,
        CommandPhase.Chaos,
        CommandPhase.Movement,
        CommandPhase.Control
    };

    private readonly List<CommandPhaseProgress> _commandPhases;
    private int _currentCommandPhaseIndex;

    public TurnController()
    {
        _commandPhases = CommandPhaseOrder
            .Select(phase => new CommandPhaseProgress(phase, CommandPhaseState.Upcoming))
            .ToList();

        InitializeState();
    }

    public bool IsTurnActive { get; private set; }

    public TurnPhase CurrentPhase { get; private set; }

    public CommandPhase? ActiveCommandPhase { get; private set; }

    public int TurnNumber { get; private set; }

    public IReadOnlyList<CommandPhaseProgress> CommandPhases => _commandPhases;

    public bool CanStartTurn => !IsTurnActive;

    public bool CanAdvancePhase
    {
        get
        {
            if (!IsTurnActive)
            {
                return false;
            }

            if (CurrentPhase == TurnPhase.Command)
            {
                return _currentCommandPhaseIndex < _commandPhases.Count;
            }

            var phaseIndex = Array.IndexOf(PhaseOrder, CurrentPhase);
            return phaseIndex >= 0 && phaseIndex < PhaseOrder.Length - 1;
        }
    }

    public bool CanEndTurn => IsTurnActive && CurrentPhase == TurnPhase.Elimination;

    public event EventHandler? StateChanged;

    public event EventHandler? TurnCompleted;

    public void StartTurn()
    {
        if (!CanStartTurn)
        {
            return;
        }

        IsTurnActive = true;
        CurrentPhase = TurnPhase.Upkeep;
        _currentCommandPhaseIndex = -1;
        ResetCommandPhases();
        ClearActiveCommandPhase();

        RaiseStateChanged();
    }

    public void AdvancePhase()
    {
        if (!CanAdvancePhase)
        {
            return;
        }

        if (CurrentPhase == TurnPhase.Command && TryAdvanceCommandPhase())
        {
            RaiseStateChanged();
            return;
        }

        if (CurrentPhase == TurnPhase.Command)
        {
            CompleteCommandPhases();
        }

        CurrentPhase = GetNextPhase(CurrentPhase);

        if (CurrentPhase == TurnPhase.Command)
        {
            ActivateCommandPhase(0);
        }
        else
        {
            ClearActiveCommandPhase();
        }

        RaiseStateChanged();
    }

    public void EndTurn()
    {
        if (!CanEndTurn)
        {
            return;
        }

        if (TurnNumber == int.MaxValue)
        {
            throw new InvalidOperationException("Turn number overflow.");
        }

        TurnNumber++;
        IsTurnActive = false;
        CurrentPhase = TurnPhase.Upkeep;
        _currentCommandPhaseIndex = -1;
        ResetCommandPhases();
        ClearActiveCommandPhase();

        TurnCompleted?.Invoke(this, EventArgs.Empty);
        RaiseStateChanged();
    }

    public void Reset()
    {
        InitializeState();
        RaiseStateChanged();
    }

    private void InitializeState()
    {
        IsTurnActive = false;
        CurrentPhase = TurnPhase.Upkeep;
        TurnNumber = 1;
        _currentCommandPhaseIndex = -1;
        ResetCommandPhases();
        ClearActiveCommandPhase();
    }

    private bool TryAdvanceCommandPhase()
    {
        if (_currentCommandPhaseIndex < 0)
        {
            ActivateCommandPhase(0);
            return true;
        }

        if (_currentCommandPhaseIndex < _commandPhases.Count - 1)
        {
            ActivateCommandPhase(_currentCommandPhaseIndex + 1);
            return true;
        }

        return false;
    }

    private void ActivateCommandPhase(int index)
    {
        if (index < 0 || index >= _commandPhases.Count)
        {
            return;
        }

        for (var i = 0; i < _commandPhases.Count; i++)
        {
            if (i < index)
            {
                _commandPhases[i].State = CommandPhaseState.Completed;
            }
            else if (i == index)
            {
                _commandPhases[i].State = CommandPhaseState.Active;
            }
            else
            {
                _commandPhases[i].State = CommandPhaseState.Upcoming;
            }
        }

        _currentCommandPhaseIndex = index;
        ActiveCommandPhase = _commandPhases[index].Phase;
    }

    private void CompleteCommandPhases()
    {
        foreach (var phase in _commandPhases)
        {
            phase.State = CommandPhaseState.Completed;
        }

        _currentCommandPhaseIndex = _commandPhases.Count - 1;
    }

    private void ResetCommandPhases()
    {
        foreach (var phase in _commandPhases)
        {
            phase.State = CommandPhaseState.Upcoming;
        }
    }

    private void ClearActiveCommandPhase()
    {
        ActiveCommandPhase = null;
    }

    private static TurnPhase GetNextPhase(TurnPhase phase)
    {
        var index = Array.IndexOf(PhaseOrder, phase);
        if (index < 0 || index >= PhaseOrder.Length - 1)
        {
            return phase;
        }

        return PhaseOrder[index + 1];
    }

    private void RaiseStateChanged() => StateChanged?.Invoke(this, EventArgs.Empty);
}
