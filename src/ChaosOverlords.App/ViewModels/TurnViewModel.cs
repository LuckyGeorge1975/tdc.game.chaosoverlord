using System;
using System.Collections.ObjectModel;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChaosOverlords.App.ViewModels;

/// <summary>
/// View model that projects the turn controller state into UI-friendly bindings.
/// </summary>
public sealed partial class TurnViewModel : ViewModelBase
{
    private readonly ITurnController _turnController;

    public TurnViewModel(ITurnController turnController)
    {
        _turnController = turnController ?? throw new ArgumentNullException(nameof(turnController));

        CommandTimeline = new ObservableCollection<CommandPhaseViewModel>(
            _turnController.CommandPhases.Select(p => new CommandPhaseViewModel(p.Phase, p.State)));

        SyncFromController();

        _turnController.StateChanged += OnControllerStateChanged;
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

    [ObservableProperty]
    private bool _isTurnActive;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPhaseDisplay))]
    private TurnPhase _currentPhase;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveCommandPhaseDisplay))]
    [NotifyPropertyChangedFor(nameof(HasActiveCommandPhase))]
    private CommandPhase? _activeCommandPhase;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TurnCounterDisplay))]
    private int _turnNumber;

    /// <summary>
    /// Ordered collection of command sub-phases exposed to the timeline UI.
    /// </summary>
    public ObservableCollection<CommandPhaseViewModel> CommandTimeline { get; }

    [RelayCommand(CanExecute = nameof(CanStartTurn))]
    private void StartTurn() => _turnController.StartTurn();

    [RelayCommand(CanExecute = nameof(CanAdvancePhase))]
    private void AdvancePhase() => _turnController.AdvancePhase();

    [RelayCommand(CanExecute = nameof(CanEndTurn))]
    private void EndTurn() => _turnController.EndTurn();

    private bool CanStartTurn() => _turnController.CanStartTurn;

    private bool CanAdvancePhase() => _turnController.CanAdvancePhase;

    private bool CanEndTurn() => _turnController.CanEndTurn;

    private void OnControllerStateChanged(object? sender, EventArgs e) => SyncFromController();

    private void SyncFromController()
    {
        IsTurnActive = _turnController.IsTurnActive;
        CurrentPhase = _turnController.CurrentPhase;
        ActiveCommandPhase = _turnController.ActiveCommandPhase;
        TurnNumber = _turnController.TurnNumber;

        var phases = _turnController.CommandPhases;
        if (CommandTimeline.Count != phases.Count)
        {
            CommandTimeline.Clear();
            foreach (var phase in phases)
            {
                CommandTimeline.Add(new CommandPhaseViewModel(phase.Phase, phase.State));
            }
        }
        else
        {
            for (var i = 0; i < phases.Count; i++)
            {
                CommandTimeline[i].State = phases[i].State;
            }
        }

        StartTurnCommand.NotifyCanExecuteChanged();
        AdvancePhaseCommand.NotifyCanExecuteChanged();
        EndTurnCommand.NotifyCanExecuteChanged();
    }

    public sealed partial class CommandPhaseViewModel : ObservableObject
    {
        public CommandPhaseViewModel(CommandPhase phase, CommandPhaseState state)
        {
            Phase = phase;
            _state = state;
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
}
