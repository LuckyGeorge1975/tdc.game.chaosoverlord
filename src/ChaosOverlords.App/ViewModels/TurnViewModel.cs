using System;
using System.Collections.ObjectModel;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChaosOverlords.App.ViewModels;

/// <summary>
/// View model that coordinates the per-turn state machine and exposes UI-friendly helpers.
/// </summary>
public sealed partial class TurnViewModel : ViewModelBase
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

    private int _currentCommandPhaseIndex = -1;

    public TurnViewModel()
    {
        CommandTimeline = new ObservableCollection<CommandPhaseViewModel>(
            CommandPhaseOrder.Select(phase => new CommandPhaseViewModel(phase)));

        ResetTurnState();
    }

    /// <summary>
    /// Descriptive title for the current phase.
    /// </summary>
    public string CurrentPhaseDisplay => CurrentPhase.ToString();

    /// <summary>
    /// Descriptive title for the current command sub-phase, if any.
    /// </summary>
    public string? ActiveCommandPhaseDisplay => ActiveCommandPhase?.ToString();

    /// <summary>
    /// Indicates whether a command sub-phase is currently active.
    /// </summary>
    public bool HasActiveCommandPhase => ActiveCommandPhase.HasValue;

    /// <summary>
    /// Human-friendly representation of the turn counter (starting at 1).
    /// </summary>
    public string TurnCounterDisplay => $"Turn {TurnNumber}";

    /// <summary>
    /// Tracks whether the turn state machine is currently active.
    /// </summary>
    [ObservableProperty]
    private bool _isTurnActive;

    /// <summary>
    /// Current top-level phase.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPhaseDisplay))]
    [NotifyCanExecuteChangedFor(nameof(AdvancePhaseCommand))]
    [NotifyCanExecuteChangedFor(nameof(EndTurnCommand))]
    private TurnPhase _currentPhase = TurnPhase.Upkeep;

    /// <summary>
    /// Current command sub-phase when the command timeline is running.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveCommandPhaseDisplay))]
    [NotifyPropertyChangedFor(nameof(HasActiveCommandPhase))]
    private CommandPhase? _activeCommandPhase;

    /// <summary>
    /// Current turn number, incremented when a turn is completed.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TurnCounterDisplay))]
    private int _turnNumber = 1;

    /// <summary>
    /// Ordered collection of command sub-phases exposed to the timeline UI.
    /// </summary>
    public ObservableCollection<CommandPhaseViewModel> CommandTimeline { get; }

    /// <summary>
    /// Starts a new turn by resetting the state machine and enabling phase advancement.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStartTurn))]
    private void StartTurn()
    {
        IsTurnActive = true;
        _currentCommandPhaseIndex = -1;
        CurrentPhase = TurnPhase.Upkeep;
        ResetCommandPhases();
    }

    /// <summary>
    /// Advances either the current command sub-phase or the major turn phase.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAdvancePhase))]
    private void AdvancePhase()
    {
        if (!IsTurnActive)
        {
            return;
        }

        if (CurrentPhase == TurnPhase.Command && TryAdvanceCommandPhase())
        {
            return;
        }

        var nextPhase = GetNextPhase(CurrentPhase);
        if (nextPhase == CurrentPhase)
        {
            return;
        }

        if (CurrentPhase == TurnPhase.Command)
        {
            CompleteCommandPhases();
        }

        CurrentPhase = nextPhase;

        if (CurrentPhase == TurnPhase.Command)
        {
            ActivateCommandPhase(0);
        }
        else
        {
            ClearActiveCommandPhase();
        }
    }

    /// <summary>
    /// Completes the turn, increments the turn counter and resets to the initial state.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanEndTurn))]
    private void EndTurn()
    {
        if (!IsTurnActive || CurrentPhase != TurnPhase.Elimination)
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
    }

    private bool CanStartTurn() => !IsTurnActive;

    private bool CanAdvancePhase()
    {
        if (!IsTurnActive)
        {
            return false;
        }

        if (CurrentPhase == TurnPhase.Command)
        {
            return _currentCommandPhaseIndex < CommandTimeline.Count;
        }

        var phaseIndex = Array.IndexOf(PhaseOrder, CurrentPhase);
        return phaseIndex >= 0 && phaseIndex < PhaseOrder.Length - 1;
    }

    private bool CanEndTurn() => IsTurnActive && CurrentPhase == TurnPhase.Elimination;

    partial void OnIsTurnActiveChanged(bool value)
    {
        StartTurnCommand.NotifyCanExecuteChanged();
        AdvancePhaseCommand.NotifyCanExecuteChanged();
        EndTurnCommand.NotifyCanExecuteChanged();
    }

    partial void OnCurrentPhaseChanged(TurnPhase value)
    {
        AdvancePhaseCommand.NotifyCanExecuteChanged();
        EndTurnCommand.NotifyCanExecuteChanged();
    }

    private void ResetTurnState()
    {
        IsTurnActive = false;
        CurrentPhase = TurnPhase.Upkeep;
        _currentCommandPhaseIndex = -1;
        ResetCommandPhases();
        ClearActiveCommandPhase();
    }

    private void ResetCommandPhases()
    {
        foreach (var phase in CommandTimeline)
        {
            phase.State = CommandPhaseState.Upcoming;
        }

        AdvancePhaseCommand.NotifyCanExecuteChanged();
    }

    private bool TryAdvanceCommandPhase()
    {
        if (_currentCommandPhaseIndex < 0)
        {
            ActivateCommandPhase(0);
            return true;
        }

        if (_currentCommandPhaseIndex < CommandTimeline.Count - 1)
        {
            ActivateCommandPhase(_currentCommandPhaseIndex + 1);
            return true;
        }

        return false;
    }

    private void ActivateCommandPhase(int index)
    {
        if (index < 0 || index >= CommandTimeline.Count)
        {
            return;
        }

        for (var i = 0; i < CommandTimeline.Count; i++)
        {
            if (i < index)
            {
                CommandTimeline[i].State = CommandPhaseState.Completed;
            }
            else if (i == index)
            {
                CommandTimeline[i].State = CommandPhaseState.Active;
            }
            else
            {
                CommandTimeline[i].State = CommandPhaseState.Upcoming;
            }
        }

        _currentCommandPhaseIndex = index;
        ActiveCommandPhase = CommandTimeline[index].Phase;
        AdvancePhaseCommand.NotifyCanExecuteChanged();
    }

    private void CompleteCommandPhases()
    {
        foreach (var phase in CommandTimeline)
        {
            phase.State = CommandPhaseState.Completed;
        }

        _currentCommandPhaseIndex = CommandTimeline.Count - 1;
        AdvancePhaseCommand.NotifyCanExecuteChanged();
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

    public sealed partial class CommandPhaseViewModel : ObservableObject
    {
        public CommandPhaseViewModel(CommandPhase phase)
        {
            Phase = phase;
            State = CommandPhaseState.Upcoming;
        }

        public CommandPhase Phase { get; }

        public string DisplayName => Phase.ToString();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsActive))]
        [NotifyPropertyChangedFor(nameof(IsCompleted))]
        private CommandPhaseState _state;

        public bool IsActive => State == CommandPhaseState.Active;

        public bool IsCompleted => State == CommandPhaseState.Completed;
    }

    public enum CommandPhaseState
    {
        Upcoming,
        Active,
        Completed
    }
}
