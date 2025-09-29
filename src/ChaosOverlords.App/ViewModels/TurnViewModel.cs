using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Events;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChaosOverlords.App.ViewModels;

/// <summary>
/// View model that projects the turn controller state into UI-friendly bindings.
/// </summary>
public sealed partial class TurnViewModel : ViewModelBase, IDisposable
{
    private readonly ITurnController _turnController;
    private readonly ITurnEventLog _eventLog;
    private bool _isDisposed;

    public TurnViewModel(ITurnController turnController, ITurnEventLog eventLog)
    {
        _turnController = turnController ?? throw new ArgumentNullException(nameof(turnController));
        _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));

        CommandTimeline = new ObservableCollection<CommandPhaseViewModel>(
            _turnController.CommandPhases.Select(p => new CommandPhaseViewModel(p.Phase, p.State)));

        TurnEvents = new ObservableCollection<TurnEventViewModel>();

        SyncFromController();

        UpdateTurnEvents();

        _turnController.StateChanged += OnControllerStateChanged;
        _eventLog.EventsChanged += OnEventLogChanged;
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

    /// <summary>
    /// Events recorded during recent turns.
    /// </summary>
    public ObservableCollection<TurnEventViewModel> TurnEvents { get; }

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

    private void OnEventLogChanged(object? sender, EventArgs e) => UpdateTurnEvents();

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

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _turnController.StateChanged -= OnControllerStateChanged;
        _eventLog.EventsChanged -= OnEventLogChanged;
        _isDisposed = true;
    }

    private void UpdateTurnEvents()
    {
        if (TurnEvents.Count > 0)
        {
            TurnEvents.Clear();
        }

        foreach (var entry in _eventLog.Events.OrderByDescending(e => e.TurnNumber).ThenByDescending(e => e.Timestamp))
        {
            TurnEvents.Add(new TurnEventViewModel(entry));
        }
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

    public sealed class TurnEventViewModel
    {
        public TurnEventViewModel(TurnEvent entry)
        {
            Entry = entry;
            Description = CreateDescription(entry);
        }

        public TurnEvent Entry { get; }

        public string Description { get; }

        private static string CreateDescription(TurnEvent entry)
        {
            var phaseText = entry.CommandPhase.HasValue
                ? $"{entry.Phase} / {entry.CommandPhase}"
                : entry.Phase.ToString();

            return string.Format(CultureInfo.CurrentCulture, "Turn {0}: [{1}] {2}", entry.TurnNumber, phaseText, entry.Description);
        }
    }
}
