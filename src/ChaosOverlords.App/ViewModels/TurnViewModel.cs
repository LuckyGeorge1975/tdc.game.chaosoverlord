using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Events;
using ChaosOverlords.Core.Domain.Game.Recruitment;
using ChaosOverlords.Core.Services;
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
    private readonly IGameSession _gameSession;
    private readonly IRecruitmentService _recruitmentService;
    private readonly ITurnEventWriter _eventWriter;
    private bool _isDisposed;
    private bool _recruitmentInitialised;

    public TurnViewModel(
        ITurnController turnController,
        ITurnEventLog eventLog,
        IGameSession gameSession,
        IRecruitmentService recruitmentService,
        ITurnEventWriter eventWriter)
    {
        _turnController = turnController ?? throw new ArgumentNullException(nameof(turnController));
        _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
        _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        _recruitmentService = recruitmentService ?? throw new ArgumentNullException(nameof(recruitmentService));
        _eventWriter = eventWriter ?? throw new ArgumentNullException(nameof(eventWriter));

        CommandTimeline = new ObservableCollection<CommandPhaseViewModel>(
            _turnController.CommandPhases.Select(p => new CommandPhaseViewModel(p.Phase, p.State)));

        TurnEvents = new ObservableCollection<TurnEventViewModel>();
        RecruitmentOptions = new ObservableCollection<RecruitmentOptionViewModel>();
        ControlledSectors = new ObservableCollection<SectorOptionViewModel>();

        SyncFromController();
        UpdateRecruitmentPanelState();
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

    [ObservableProperty]
    private bool _isRecruitmentPanelVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRecruitmentStatusMessage))]
    private string? _recruitmentStatusMessage;

    [ObservableProperty]
    private SectorOptionViewModel? _selectedSector;

    /// <summary>
    /// Indicates whether any recruitment status message should be shown in the UI.
    /// </summary>
    public bool HasRecruitmentStatusMessage => !string.IsNullOrWhiteSpace(RecruitmentStatusMessage);

    /// <summary>
    /// Ordered collection of command sub-phases exposed to the timeline UI.
    /// </summary>
    public ObservableCollection<CommandPhaseViewModel> CommandTimeline { get; }

    /// <summary>
    /// Events recorded during recent turns.
    /// </summary>
    public ObservableCollection<TurnEventViewModel> TurnEvents { get; }

    /// <summary>
    /// Recruitment options available during the hire phase.
    /// </summary>
    public ObservableCollection<RecruitmentOptionViewModel> RecruitmentOptions { get; }

    /// <summary>
    /// Sectors controlled by the active player that can receive recruits.
    /// </summary>
    public ObservableCollection<SectorOptionViewModel> ControlledSectors { get; }

    [RelayCommand(CanExecute = nameof(CanStartTurn))]
    private void StartTurn() => _turnController.StartTurn();

    [RelayCommand(CanExecute = nameof(CanAdvancePhase))]
    private void AdvancePhase() => _turnController.AdvancePhase();

    [RelayCommand(CanExecute = nameof(CanEndTurn))]
    private void EndTurn() => _turnController.EndTurn();

    [RelayCommand(CanExecute = nameof(CanHire))]
    private void Hire(RecruitmentOptionViewModel option)
    {
        if (option is null || SelectedSector is null)
        {
            return;
        }

        EnsureSessionInitialised();

        var state = _gameSession.GameState;
        var playerId = state.CurrentPlayer.Id;
        var targetSectorId = SelectedSector.SectorId;
        var result = _recruitmentService.Hire(state, playerId, option.OptionId, targetSectorId, _turnController.TurnNumber);

        ApplyRecruitmentSnapshot(result.Pool);
        LoadControlledSectors(playerId);

        if (result.Status == RecruitmentActionStatus.Success && result.Option is not null)
        {
            RecruitmentStatusMessage = string.Format(
                CultureInfo.CurrentCulture,
                "Hired {0} into sector {1}.",
                result.Option.GangName,
                targetSectorId);

            _eventWriter.Write(
                _turnController.TurnNumber,
                TurnPhase.Hire,
                TurnEventType.Recruitment,
                string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} hired {1} into {2}.",
                    result.Pool.PlayerName,
                    result.Option.GangName,
                    targetSectorId));
        }
        else
        {
            var reason = string.IsNullOrWhiteSpace(result.FailureReason)
                ? "Recruitment failed."
                : result.FailureReason;

            RecruitmentStatusMessage = reason;

            _eventWriter.Write(
                _turnController.TurnNumber,
                TurnPhase.Hire,
                TurnEventType.Recruitment,
                string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} failed to hire in {1} ({2}).",
                    result.Pool.PlayerName,
                    targetSectorId,
                    reason));
        }

        RefreshRecruitmentCommands();
    }

    [RelayCommand(CanExecute = nameof(CanDecline))]
    private void Decline(RecruitmentOptionViewModel option)
    {
        if (option is null)
        {
            return;
        }

        EnsureSessionInitialised();

        var state = _gameSession.GameState;
        var playerId = state.CurrentPlayer.Id;
        var result = _recruitmentService.Decline(state, playerId, option.OptionId, _turnController.TurnNumber);

        ApplyRecruitmentSnapshot(result.Pool);
        LoadControlledSectors(playerId);

        if (result.Status == RecruitmentActionStatus.Success && result.Option is not null)
        {
            RecruitmentStatusMessage = string.Format(
                CultureInfo.CurrentCulture,
                "Declined {0}. Replacement will arrive next turn.",
                result.Option.GangName);

            _eventWriter.Write(
                _turnController.TurnNumber,
                TurnPhase.Hire,
                TurnEventType.Recruitment,
                string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} declined {1}.",
                    result.Pool.PlayerName,
                    result.Option.GangName));
        }
        else
        {
            var reason = string.IsNullOrWhiteSpace(result.FailureReason)
                ? "Decline failed."
                : result.FailureReason;

            RecruitmentStatusMessage = reason;

            _eventWriter.Write(
                _turnController.TurnNumber,
                TurnPhase.Hire,
                TurnEventType.Recruitment,
                string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} failed to decline ({1}).",
                    result.Pool.PlayerName,
                    reason));
        }

        RefreshRecruitmentCommands();
    }

    private bool CanStartTurn() => _turnController.CanStartTurn;

    private bool CanAdvancePhase() => _turnController.CanAdvancePhase;

    private bool CanEndTurn() => _turnController.CanEndTurn;

    private bool CanHire(RecruitmentOptionViewModel? option)
        => option is not null && option.IsAvailable && SelectedSector is not null;

    private bool CanDecline(RecruitmentOptionViewModel? option)
        => option is not null && option.IsAvailable;

    private void OnControllerStateChanged(object? sender, EventArgs e)
    {
        SyncFromController();
        UpdateRecruitmentPanelState();
    }

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

    private void UpdateRecruitmentPanelState()
    {
        if (!_turnController.IsTurnActive || _turnController.CurrentPhase != TurnPhase.Hire)
        {
            HideRecruitmentPanel();
            return;
        }

        if (!_gameSession.IsInitialized)
        {
            return;
        }

        var state = _gameSession.GameState;
        if (state.CurrentPlayer.Id != state.PrimaryPlayerId)
        {
            HideRecruitmentPanel();
            return;
        }

        if (_recruitmentInitialised && RecruitmentOptions.Count > 0)
        {
            return;
        }

        var snapshot = _recruitmentService.EnsurePool(state, state.CurrentPlayer.Id, _turnController.TurnNumber);
        ApplyRecruitmentSnapshot(snapshot);
        LoadControlledSectors(state.CurrentPlayer.Id);

        if (string.IsNullOrWhiteSpace(RecruitmentStatusMessage))
        {
            RecruitmentStatusMessage = "Select a sector and recruit a gang.";
        }
    }

    private void ApplyRecruitmentSnapshot(RecruitmentPoolSnapshot snapshot)
    {
        var ordered = snapshot.Options.OrderBy(o => o.SlotIndex).ToList();
        if (RecruitmentOptions.Count != ordered.Count)
        {
            RecruitmentOptions.Clear();
            foreach (var option in ordered)
            {
                RecruitmentOptions.Add(new RecruitmentOptionViewModel(option, HireCommand, DeclineCommand));
            }
        }
        else
        {
            for (var i = 0; i < ordered.Count; i++)
            {
                RecruitmentOptions[i].Update(ordered[i]);
            }
        }

        _recruitmentInitialised = true;
        IsRecruitmentPanelVisible = true;
        RefreshRecruitmentCommands();
    }

    private void LoadControlledSectors(Guid playerId)
    {
        var sectors = _gameSession.GameState.Game.Sectors.Values
            .Where(s => s.ControllingPlayerId == playerId)
            .OrderBy(s => s.Id, StringComparer.Ordinal)
            .Select(s => new SectorOptionViewModel(s.Id))
            .ToList();

        ControlledSectors.Clear();
        foreach (var sector in sectors)
        {
            ControlledSectors.Add(sector);
        }

        if (ControlledSectors.Count == 0)
        {
            SelectedSector = null;
            if (string.IsNullOrWhiteSpace(RecruitmentStatusMessage))
            {
                RecruitmentStatusMessage = "No controlled sectors available for deployment.";
            }
        }
        else
        {
            if (SelectedSector is null || ControlledSectors.All(s => s.SectorId != SelectedSector.SectorId))
            {
                SelectedSector = ControlledSectors[0];
            }
        }
    }

    private void HideRecruitmentPanel()
    {
        if (!IsRecruitmentPanelVisible && RecruitmentOptions.Count == 0)
        {
            return;
        }

        IsRecruitmentPanelVisible = false;
        RecruitmentOptions.Clear();
        ControlledSectors.Clear();
        SelectedSector = null;
        _recruitmentInitialised = false;
        RecruitmentStatusMessage = null;
        RefreshRecruitmentCommands();
    }

    private void RefreshRecruitmentCommands()
    {
        HireCommand.NotifyCanExecuteChanged();
        DeclineCommand.NotifyCanExecuteChanged();
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

    private void EnsureSessionInitialised()
    {
        if (!_gameSession.IsInitialized)
        {
            throw new InvalidOperationException("Game session has not been initialised.");
        }
    }

    partial void OnSelectedSectorChanged(SectorOptionViewModel? oldValue, SectorOptionViewModel? newValue)
    {
        if (oldValue != newValue)
        {
            RefreshRecruitmentCommands();
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

    public sealed partial class RecruitmentOptionViewModel : ObservableObject
    {
        public RecruitmentOptionViewModel(
            RecruitmentOptionSnapshot snapshot,
            IRelayCommand<RecruitmentOptionViewModel> hireCommand,
            IRelayCommand<RecruitmentOptionViewModel> declineCommand)
        {
            SlotIndex = snapshot.SlotIndex;
            HireCommand = hireCommand ?? throw new ArgumentNullException(nameof(hireCommand));
            DeclineCommand = declineCommand ?? throw new ArgumentNullException(nameof(declineCommand));
            Update(snapshot);
        }

        public int SlotIndex { get; }

        public IRelayCommand<RecruitmentOptionViewModel> HireCommand { get; }

        public IRelayCommand<RecruitmentOptionViewModel> DeclineCommand { get; }

        [ObservableProperty]
        private Guid _optionId;

        [ObservableProperty]
        private string _gangName = string.Empty;

        [ObservableProperty]
        private int _hiringCost;

        [ObservableProperty]
        private int _upkeepCost;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StatusDisplay))]
        [NotifyPropertyChangedFor(nameof(IsAvailable))]
        [NotifyPropertyChangedFor(nameof(IsDeclined))]
        [NotifyPropertyChangedFor(nameof(IsHired))]
        private RecruitmentOptionState _state;

        public string StatusDisplay => State.ToString();

        public bool IsAvailable => State == RecruitmentOptionState.Available;

        public bool IsDeclined => State == RecruitmentOptionState.Declined;

        public bool IsHired => State == RecruitmentOptionState.Hired;

        public void Update(RecruitmentOptionSnapshot snapshot)
        {
            OptionId = snapshot.OptionId;
            GangName = snapshot.GangName;
            HiringCost = snapshot.HiringCost;
            UpkeepCost = snapshot.UpkeepCost;
            State = snapshot.State;
        }
    }

    public sealed class SectorOptionViewModel
    {
        public SectorOptionViewModel(string sectorId)
        {
            SectorId = sectorId ?? throw new ArgumentNullException(nameof(sectorId));
        }

        public string SectorId { get; }

        public string DisplayName => SectorId;
    }
}
