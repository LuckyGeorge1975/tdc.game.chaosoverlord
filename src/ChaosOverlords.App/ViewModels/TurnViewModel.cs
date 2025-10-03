using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using ChaosOverlords.App.Messaging;
using ChaosOverlords.App.ViewModels.Sections;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Commands;
using ChaosOverlords.Core.Domain.Game.Economy;
using ChaosOverlords.Core.Domain.Game.Events;
using ChaosOverlords.Core.Domain.Game.Recruitment;
using ChaosOverlords.Core.Services;
using ChaosOverlords.Core.Services.Messaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChaosOverlords.App.ViewModels;

/// <summary>
///     View model that projects the turn controller state into UI-friendly bindings.
/// </summary>
public sealed partial class TurnViewModel : ViewModelBase, IDisposable
{
    private readonly ICommandQueueService _commandQueueService;
    private readonly IDataService? _dataService;
    private readonly ITurnEventLog _eventLog;
    private readonly ITurnEventWriter _eventWriter;
    private readonly IFinancePreviewService _financePreviewService;
    private readonly IGameSession _gameSession;
    private readonly ILogPathProvider _logPathProvider;
    private readonly IMessageHub _messageHub;
    private readonly IRecruitmentService _recruitmentService;
    private readonly IResearchService _researchService;
    private readonly ITurnController _turnController;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveCommandPhaseDisplay))]
    [NotifyPropertyChangedFor(nameof(HasActiveCommandPhase))]
    private CommandPhase? _activeCommandPhase;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CityNetChangeDisplay))]
    private int _cityNetChange;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasCommandStatusMessage))]
    private string? _commandStatusMessage;

    // Preview strings for Control and Influence outcomes.
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasControlPreview))]
    private string? _controlPreview;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CurrentPhaseDisplay))]
    private TurnPhase _currentPhase;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(FinancePreviewHeading))]
    private string? _financePreviewPlayerName;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasInfluencePreview))]
    private string? _influencePreview;

    [ObservableProperty] private bool _isCommandPanelVisible;

    private bool _isDisposed;

    [ObservableProperty] private bool _isFinancePreviewVisible;

    [ObservableProperty] private bool _isRecruitmentPanelVisible;

    [ObservableProperty] private bool _isTurnActive;

    private bool _recruitmentInitialised;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasRecruitmentStatusMessage))]
    private string? _recruitmentStatusMessage;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasResearchPreview))]
    private string? _researchPreview;

    [ObservableProperty] private string? _researchProjectId;

    [ObservableProperty] private GangOptionViewModel? _selectedGang;

    [ObservableProperty] private SectorOptionViewModel? _selectedMovementTarget;

    [ObservableProperty] private SectorOptionViewModel? _selectedSector;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(TurnCounterDisplay))]
    private int _turnNumber;

    public TurnViewModel(
        ITurnController turnController,
        ITurnEventLog eventLog,
        IGameSession gameSession,
        IRecruitmentService recruitmentService,
        ITurnEventWriter eventWriter,
        ICommandQueueService commandQueueService,
        IFinancePreviewService financePreviewService,
        IResearchService researchService,
        IMessageHub messageHub,
        ILogPathProvider logPathProvider)
    {
        _turnController = turnController ?? throw new ArgumentNullException(nameof(turnController));
        _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
        _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        _recruitmentService = recruitmentService ?? throw new ArgumentNullException(nameof(recruitmentService));
        _eventWriter = eventWriter ?? throw new ArgumentNullException(nameof(eventWriter));
        _commandQueueService = commandQueueService ?? throw new ArgumentNullException(nameof(commandQueueService));
        _financePreviewService =
            financePreviewService ?? throw new ArgumentNullException(nameof(financePreviewService));
        _researchService = researchService ?? throw new ArgumentNullException(nameof(researchService));
        _dataService = null;
        _messageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
        _logPathProvider = logPathProvider ?? throw new ArgumentNullException(nameof(logPathProvider));

        TurnEvents = new ObservableCollection<TurnEventViewModel>();
        RecruitmentOptions = new ObservableCollection<RecruitmentOptionViewModel>();
        ControlledSectors = new ObservableCollection<SectorOptionViewModel>();
        AvailableGangs = new ObservableCollection<GangOptionViewModel>();
        MovementTargets = new ObservableCollection<SectorOptionViewModel>();
        QueuedCommands = new ObservableCollection<QueuedCommandViewModel>();
        CityFinanceCategories = new ObservableCollection<FinanceCategoryViewModel>();
        SectorFinance = new ObservableCollection<FinanceSectorViewModel>();

        TurnManagement = new TurnManagementSectionViewModel(
            _messageHub,
            StartTurnCommand,
            AdvancePhaseCommand,
            EndTurnCommand);

        CommandTimeline = new CommandTimelineSectionViewModel(_messageHub);

        FinancePreview = new FinancePreviewSectionViewModel(this);
        CommandQueue = new CommandQueueSectionViewModel(this);
        Recruitment = new RecruitmentSectionViewModel(this);
        TurnEventsPanel = new TurnEventsSectionViewModel(TurnEvents, OpenLogsFolder);

        SyncFromController();
        UpdateRecruitmentPanelState();
        UpdateCommandPanelState();
        UpdateTurnEvents();

        _turnController.StateChanged += OnControllerStateChanged;
        _eventLog.EventsChanged += OnEventLogChanged;

        // Try to load suggestions if data service is available via the other constructor.
        TryLoadResearchSuggestions();
    }

    public TurnViewModel(
        ITurnController turnController,
        ITurnEventLog eventLog,
        IGameSession gameSession,
        IRecruitmentService recruitmentService,
        ITurnEventWriter eventWriter,
        ICommandQueueService commandQueueService,
        IFinancePreviewService financePreviewService,
        IResearchService researchService,
        IDataService dataService,
        IMessageHub messageHub,
        ILogPathProvider logPathProvider)
        : this(turnController, eventLog, gameSession, recruitmentService, eventWriter, commandQueueService,
            financePreviewService, researchService, messageHub, logPathProvider)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        TryLoadResearchSuggestions();
    }

    /// <summary>
    ///     Descriptive title for the current phase.
    /// </summary>
    public string CurrentPhaseDisplay => CurrentPhase.ToString();

    /// <summary>
    ///     Descriptive title for the current command sub-phase, if any.
    /// </summary>
    public string? ActiveCommandPhaseDisplay => ActiveCommandPhase?.ToString();

    /// <summary>
    ///     Indicates whether a command sub-phase is currently active.
    /// </summary>
    public bool HasActiveCommandPhase => ActiveCommandPhase.HasValue;

    /// <summary>
    ///     Human-friendly representation of the turn counter (starting at 1).
    /// </summary>
    public string TurnCounterDisplay => $"Turn {TurnNumber}";

    public TurnManagementSectionViewModel TurnManagement { get; }

    public CommandTimelineSectionViewModel CommandTimeline { get; }

    public FinancePreviewSectionViewModel FinancePreview { get; }

    public CommandQueueSectionViewModel CommandQueue { get; }

    public RecruitmentSectionViewModel Recruitment { get; }

    public TurnEventsSectionViewModel TurnEventsPanel { get; }

    /// <summary>
    ///     Indicates whether any recruitment status message should be shown in the UI.
    /// </summary>
    public bool HasRecruitmentStatusMessage => !string.IsNullOrWhiteSpace(RecruitmentStatusMessage);

    /// <summary>
    ///     Indicates whether any command status message should be shown in the UI.
    /// </summary>
    public bool HasCommandStatusMessage => !string.IsNullOrWhiteSpace(CommandStatusMessage);

    public bool HasControlPreview => !string.IsNullOrWhiteSpace(ControlPreview);
    public bool HasInfluencePreview => !string.IsNullOrWhiteSpace(InfluencePreview);
    public bool HasResearchPreview => !string.IsNullOrWhiteSpace(ResearchPreview);

    public ObservableCollection<ResearchSuggestionViewModel> ResearchSuggestions { get; } = new();

    /// <summary>
    ///     Heading describing which player's finances are shown.
    /// </summary>
    public string FinancePreviewHeading => FinancePreviewPlayerName is null
        ? "Finance Preview"
        : string.Format(CultureInfo.CurrentCulture, "{0}'s Finance Preview", FinancePreviewPlayerName);

    /// <summary>
    ///     Signed city-wide net change formatted for display.
    /// </summary>
    public string CityNetChangeDisplay => FormatAmount(CityNetChange);

    /// <summary>
    ///     Events recorded during recent turns.
    /// </summary>
    public ObservableCollection<TurnEventViewModel> TurnEvents { get; }

    /// <summary>
    ///     Recruitment options available during the hire phase.
    /// </summary>
    public ObservableCollection<RecruitmentOptionViewModel> RecruitmentOptions { get; }

    /// <summary>
    ///     Sectors controlled by the active player that can receive recruits.
    /// </summary>
    public ObservableCollection<SectorOptionViewModel> ControlledSectors { get; }

    /// <summary>
    ///     Gangs controlled by the active player available for command assignment.
    /// </summary>
    public ObservableCollection<GangOptionViewModel> AvailableGangs { get; }

    /// <summary>
    ///     Valid movement targets for the selected gang during the command phase.
    /// </summary>
    public ObservableCollection<SectorOptionViewModel> MovementTargets { get; }

    /// <summary>
    ///     Commands currently queued for the active player.
    /// </summary>
    public ObservableCollection<QueuedCommandViewModel> QueuedCommands { get; }

    /// <summary>
    ///     City-wide finance categories for the active player.
    /// </summary>
    public ObservableCollection<FinanceCategoryViewModel> CityFinanceCategories { get; }

    /// <summary>
    ///     Finance projections for each controlled sector.
    /// </summary>
    public ObservableCollection<FinanceSectorViewModel> SectorFinance { get; }

    public void Dispose()
    {
        if (_isDisposed) return;

        _turnController.StateChanged -= OnControllerStateChanged;
        _eventLog.EventsChanged -= OnEventLogChanged;
        TurnManagement.Dispose();
        CommandTimeline.Dispose();
        FinancePreview.Dispose();
        CommandQueue.Dispose();
        Recruitment.Dispose();
        TurnEventsPanel.Dispose();
        _isDisposed = true;
    }

    private void TryLoadResearchSuggestions()
    {
        if (_dataService is null) return;

        try
        {
            var items = _dataService.GetItemsAsync().GetAwaiter().GetResult();
            ResearchSuggestions.Clear();
            foreach (var item in items.OrderBy(i => i.Name, StringComparer.Ordinal))
                ResearchSuggestions.Add(new ResearchSuggestionViewModel(item.Name, item.ResearchCost));
        }
        catch
        {
            // Non-fatal: suggestions are optional for gameplay; swallow exceptions here.
        }
    }

    partial void OnResearchProjectIdChanged(string? value)
    {
        QueueResearchCommand.NotifyCanExecuteChanged();
        UpdatePreviews();
    }

    [RelayCommand(CanExecute = nameof(CanStartTurn))]
    private void StartTurn()
    {
        _turnController.StartTurn();
    }

    [RelayCommand(CanExecute = nameof(CanAdvancePhase))]
    private void AdvancePhase()
    {
        _turnController.AdvancePhase();
    }

    [RelayCommand(CanExecute = nameof(CanEndTurn))]
    private void EndTurn()
    {
        _turnController.EndTurn();
    }

    private void OpenLogsFolder()
    {
        try
        {
            var dir = _logPathProvider.GetLogDirectory();
            if (OperatingSystem.IsWindows())
                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true,
                    Verb = "open"
                });
            else if (OperatingSystem.IsMacOS())
                Process.Start("open", dir);
            else if (OperatingSystem.IsLinux()) Process.Start("xdg-open", dir);
        }
        catch
        {
            // Swallow any failures to avoid crashing the UI; the button is a convenience.
        }
    }

    [RelayCommand(CanExecute = nameof(CanHire))]
    private void Hire(RecruitmentOptionViewModel option)
    {
        if (option is null || SelectedSector is null) return;

        EnsureSessionInitialised();

        var state = _gameSession.GameState;
        var playerId = state.CurrentPlayer.Id;
        var targetSectorId = SelectedSector.SectorId;
        var result =
            _recruitmentService.Hire(state, playerId, option.OptionId, targetSectorId, _turnController.TurnNumber);

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
        if (option is null) return;

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

    [RelayCommand(CanExecute = nameof(CanQueueMove))]
    private void QueueMove()
    {
        if (SelectedGang is null || SelectedMovementTarget is null) return;

        EnsureSessionInitialised();

        var state = _gameSession.GameState;
        var playerId = state.CurrentPlayer.Id;
        var result = _commandQueueService.QueueMove(
            state,
            playerId,
            SelectedGang.GangId,
            SelectedMovementTarget.SectorId,
            _turnController.TurnNumber);

        ApplyCommandQueueResult(result);
    }

    [RelayCommand(CanExecute = nameof(CanQueueSimpleCommand))]
    private void QueueControl()
    {
        if (SelectedGang is null) return;

        EnsureSessionInitialised();

        var state = _gameSession.GameState;
        var playerId = state.CurrentPlayer.Id;
        var result = _commandQueueService.QueueControl(
            state,
            playerId,
            SelectedGang.GangId,
            SelectedGang.SectorId,
            _turnController.TurnNumber);

        ApplyCommandQueueResult(result);
    }

    [RelayCommand(CanExecute = nameof(CanQueueSimpleCommand))]
    private void QueueChaos()
    {
        if (SelectedGang is null) return;

        EnsureSessionInitialised();

        var state = _gameSession.GameState;
        var playerId = state.CurrentPlayer.Id;
        var result = _commandQueueService.QueueChaos(
            state,
            playerId,
            SelectedGang.GangId,
            SelectedGang.SectorId,
            _turnController.TurnNumber);

        ApplyCommandQueueResult(result);
    }

    [RelayCommand(CanExecute = nameof(CanQueueSimpleCommand))]
    private void QueueInfluence()
    {
        EnsureSessionInitialised();

        if (SelectedGang is null) return;

        var state = _gameSession.GameState;
        var playerId = state.CurrentPlayer.Id;
        var targetSectorId = SelectedSector?.SectorId ?? SelectedGang.SectorId;
        var result = _commandQueueService.QueueInfluence(state, playerId, SelectedGang.GangId, targetSectorId,
            _turnController.TurnNumber);
        ApplyCommandQueueResult(result);
    }

    [RelayCommand(CanExecute = nameof(CanQueueResearch))]
    private void QueueResearch()
    {
        EnsureSessionInitialised();
        if (SelectedGang is null || string.IsNullOrWhiteSpace(ResearchProjectId)) return;

        var state = _gameSession.GameState;
        var playerId = state.CurrentPlayer.Id;
        var result = _commandQueueService.QueueResearch(state, playerId, SelectedGang.GangId, ResearchProjectId!,
            _turnController.TurnNumber);
        ApplyCommandQueueResult(result);
    }

    [RelayCommand(CanExecute = nameof(CanRemoveQueuedCommand))]
    private void RemoveQueuedCommand(QueuedCommandViewModel command)
    {
        if (command is null) return;

        EnsureSessionInitialised();

        var state = _gameSession.GameState;
        var playerId = state.CurrentPlayer.Id;
        var result = _commandQueueService.Remove(state, playerId, command.GangId, _turnController.TurnNumber);

        ApplyCommandQueueResult(result);
    }

    private bool CanStartTurn()
    {
        return _turnController.CanStartTurn;
    }

    private bool CanAdvancePhase()
    {
        return _turnController.CanAdvancePhase;
    }

    private bool CanEndTurn()
    {
        return _turnController.CanEndTurn;
    }

    private bool CanHire(RecruitmentOptionViewModel? option)
    {
        return option is not null && option.IsAvailable && SelectedSector is not null;
    }

    private bool CanDecline(RecruitmentOptionViewModel? option)
    {
        return option is not null && option.IsAvailable;
    }

    private bool CanQueueMove()
    {
        return IsCommandPhaseInteractive() && SelectedGang is not null && SelectedMovementTarget is not null;
    }

    private bool CanQueueSimpleCommand()
    {
        return IsCommandPhaseInteractive() && SelectedGang is not null;
    }

    private bool CanQueueResearch()
    {
        return IsCommandPhaseInteractive() && SelectedGang is not null && !string.IsNullOrWhiteSpace(ResearchProjectId);
    }

    private bool CanRemoveQueuedCommand(QueuedCommandViewModel? command)
    {
        return IsCommandPhaseInteractive() && command is not null;
    }

    private void OnControllerStateChanged(object? sender, EventArgs e)
    {
        SyncFromController();
        UpdateRecruitmentPanelState();
        UpdateCommandPanelState();
        UpdatePreviews();
    }

    private void OnEventLogChanged(object? sender, EventArgs e)
    {
        UpdateTurnEvents();
    }

    private void SyncFromController()
    {
        IsTurnActive = _turnController.IsTurnActive;
        CurrentPhase = _turnController.CurrentPhase;
        ActiveCommandPhase = _turnController.ActiveCommandPhase;
        TurnNumber = _turnController.TurnNumber;

        var phases = _turnController.CommandPhases;
        PublishCommandTimeline(phases);
        PublishTurnSummary();

        StartTurnCommand.NotifyCanExecuteChanged();
        AdvancePhaseCommand.NotifyCanExecuteChanged();
        EndTurnCommand.NotifyCanExecuteChanged();
        RefreshCommandCommands();
        UpdateFinancePreview();
    }

    private void PublishTurnSummary()
    {
        var message = new TurnSummaryChangedMessage(TurnNumber, CurrentPhase, ActiveCommandPhase, IsTurnActive);
        _messageHub.Publish(message);
    }

    private void PublishCommandTimeline(IReadOnlyList<CommandPhaseProgress> phases)
    {
        if (phases is null) throw new ArgumentNullException(nameof(phases));

        var snapshots = phases
            .Select(phase => new CommandPhaseSnapshot(phase.Phase, phase.State))
            .ToArray();

        _messageHub.Publish(new CommandTimelineUpdatedMessage(snapshots));
    }

    private void UpdateRecruitmentPanelState()
    {
        if (!_turnController.IsTurnActive || _turnController.CurrentPhase != TurnPhase.Hire)
        {
            HideRecruitmentPanel();
            return;
        }

        if (!_gameSession.IsInitialized) return;

        var state = _gameSession.GameState;
        if (state.CurrentPlayer.Id != state.PrimaryPlayerId)
        {
            HideRecruitmentPanel();
            return;
        }

        if (_recruitmentInitialised && RecruitmentOptions.Count > 0) return;

        var snapshot = _recruitmentService.EnsurePool(state, state.CurrentPlayer.Id, _turnController.TurnNumber);
        ApplyRecruitmentSnapshot(snapshot);
        LoadControlledSectors(state.CurrentPlayer.Id);

        if (string.IsNullOrWhiteSpace(RecruitmentStatusMessage))
            RecruitmentStatusMessage = "Select a sector and recruit a gang.";
    }

    private void UpdateCommandPanelState()
    {
        if (!_gameSession.IsInitialized)
        {
            HideCommandPanel();
            return;
        }

        var state = _gameSession.GameState;
        if (state.CurrentPlayer.Id != state.PrimaryPlayerId)
        {
            HideCommandPanel();
            return;
        }

        // Keep controlled sectors in sync for selecting Influence targets.
        LoadControlledSectors(state.CurrentPlayer.Id);
        LoadAvailableGangs(state);
        var snapshot = _commandQueueService.GetQueue(state, state.CurrentPlayer.Id);
        ApplyQueueSnapshot(snapshot);
        UpdateMovementTargets(state);
        RefreshCommandCommands();
        UpdateFinancePreview();

        if (IsCommandPhaseInteractive())
            IsCommandPanelVisible = true;
        else
            HideCommandPanel();
    }

    private void UpdateFinancePreview()
    {
        if (!_gameSession.IsInitialized)
        {
            ClearFinancePreview();
            return;
        }

        var state = _gameSession.GameState;
        if (state.PrimaryPlayerId == Guid.Empty)
        {
            ClearFinancePreview();
            return;
        }

        FinanceProjection projection;
        try
        {
            projection = _financePreviewService.BuildProjection(state, state.PrimaryPlayerId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in BuildProjection: {ex}");
            ClearFinancePreview();
            return;
        }

        FinancePreviewPlayerName = projection.PlayerName;
        CityNetChange = projection.NetCashAdjustment;
        ApplyCityCategories(projection.CityCategories);
        ApplySectorProjections(projection.Sectors);
        IsFinancePreviewVisible = true;
    }

    private void ApplyCityCategories(IReadOnlyList<FinanceCategory> categories)
    {
        CityFinanceCategories.Clear();
        foreach (var category in categories) CityFinanceCategories.Add(new FinanceCategoryViewModel(category));
    }

    private void ApplySectorProjections(IReadOnlyList<FinanceSectorProjection> sectors)
    {
        SectorFinance.Clear();
        foreach (var sector in sectors) SectorFinance.Add(new FinanceSectorViewModel(sector));
    }

    private void ClearFinancePreview()
    {
        if (CityFinanceCategories.Count > 0) CityFinanceCategories.Clear();

        if (SectorFinance.Count > 0) SectorFinance.Clear();

        CityNetChange = 0;
        FinancePreviewPlayerName = null;
        IsFinancePreviewVisible = false;
    }

    private void ApplyRecruitmentSnapshot(RecruitmentPoolSnapshot snapshot)
    {
        var ordered = snapshot.Options.OrderBy(o => o.SlotIndex).ToList();
        if (RecruitmentOptions.Count != ordered.Count)
        {
            RecruitmentOptions.Clear();
            foreach (var option in ordered)
                RecruitmentOptions.Add(new RecruitmentOptionViewModel(option, HireCommand, DeclineCommand));
        }
        else
        {
            for (var i = 0; i < ordered.Count; i++) RecruitmentOptions[i].Update(ordered[i]);
        }

        _recruitmentInitialised = true;
        IsRecruitmentPanelVisible = true;
        RefreshRecruitmentCommands();
        UpdateFinancePreview();
    }

    private void ApplyCommandQueueResult(CommandQueueResult result)
    {
        CommandStatusMessage = result.Message;
        ApplyQueueSnapshot(result.Snapshot);
        RefreshCommandCommands();
        UpdateFinancePreview();
    }

    private void ApplyQueueSnapshot(CommandQueueSnapshot snapshot)
    {
        QueuedCommands.Clear();
        foreach (var entry in snapshot.Commands) QueuedCommands.Add(new QueuedCommandViewModel(entry));
    }

    private void LoadControlledSectors(Guid playerId)
    {
        var sectors = _gameSession.GameState.Game.Sectors.Values
            .Where(s => s.ControllingPlayerId == playerId)
            .OrderBy(s => s.Id, StringComparer.Ordinal)
            .Select(s => new SectorOptionViewModel(s.Id))
            .ToList();

        ControlledSectors.Clear();
        foreach (var sector in sectors) ControlledSectors.Add(sector);

        if (ControlledSectors.Count == 0)
        {
            SelectedSector = null;
            if (string.IsNullOrWhiteSpace(RecruitmentStatusMessage))
                RecruitmentStatusMessage = "No controlled sectors available for deployment.";
        }
        else
        {
            if (SelectedSector is null || ControlledSectors.All(s => s.SectorId != SelectedSector.SectorId))
                SelectedSector = ControlledSectors[0];
        }
    }

    private void LoadAvailableGangs(GameState state)
    {
        var playerId = state.CurrentPlayer.Id;
        var gangs = state.Game.Gangs.Values
            .Where(g => g.OwnerId == playerId)
            .OrderBy(g => g.Data.Name, StringComparer.Ordinal)
            .Select(g => new GangOptionViewModel(g.Id, g.Data.Name, g.SectorId))
            .ToList();

        var selectedId = SelectedGang?.GangId;
        AvailableGangs.Clear();
        foreach (var gang in gangs) AvailableGangs.Add(gang);

        if (selectedId.HasValue)
        {
            var existing = AvailableGangs.FirstOrDefault(g => g.GangId == selectedId.Value);
            if (existing is not null && !ReferenceEquals(existing, SelectedGang)) SelectedGang = existing;
        }

        if (SelectedGang is null && AvailableGangs.Count > 0)
            SelectedGang = AvailableGangs[0];
        else if (AvailableGangs.Count == 0) SelectedGang = null;
    }

    private void UpdateMovementTargets(GameState state)
    {
        MovementTargets.Clear();

        if (SelectedGang is null)
        {
            SelectedMovementTarget = null;
            return;
        }

        if (!state.Game.TryGetGang(SelectedGang.GangId, out var gang) || gang is null)
        {
            SelectedMovementTarget = null;
            return;
        }

        var ownerId = gang.OwnerId;
        // Build a lookup dictionary for gang counts per sector/owner
        var sectorOwnerGangCounts = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
        foreach (var g in state.Game.Gangs.Values)
        {
            if (!sectorOwnerGangCounts.TryGetValue(g.SectorId, out var ownerDict))
            {
                ownerDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                sectorOwnerGangCounts[g.SectorId] = ownerDict;
            }
            if (!ownerDict.ContainsKey(g.OwnerId))
            {
                ownerDict[g.OwnerId] = 0;
            }
            ownerDict[g.OwnerId]++;
        }
        var targets = state.Game.Sectors.Values
            .Where(s => SectorGrid.AreAdjacent(gang.SectorId, s.Id))
            .Where(s =>
            {
                if (sectorOwnerGangCounts.TryGetValue(s.Id, out var ownerDict) &&
                    ownerDict.TryGetValue(ownerId, out var count))
                {
                    return count < 6;
                }
                return true; // No gangs of this owner in this sector
            })
            .OrderBy(s => s.Id, StringComparer.Ordinal)
            .Select(s => new SectorOptionViewModel(s.Id))
            .ToList();

        foreach (var target in targets) MovementTargets.Add(target);

        if (MovementTargets.Count == 0)
            SelectedMovementTarget = null;
        else if (SelectedMovementTarget is null ||
                 MovementTargets.All(s => s.SectorId != SelectedMovementTarget.SectorId))
            SelectedMovementTarget = MovementTargets[0];
    }

    private void HideRecruitmentPanel()
    {
        if (!IsRecruitmentPanelVisible && RecruitmentOptions.Count == 0) return;

        IsRecruitmentPanelVisible = false;
        RecruitmentOptions.Clear();
        ControlledSectors.Clear();
        SelectedSector = null;
        _recruitmentInitialised = false;
        RecruitmentStatusMessage = null;
        RefreshRecruitmentCommands();
    }

    private void HideCommandPanel()
    {
        if (!IsCommandPanelVisible && AvailableGangs.Count == 0 && QueuedCommands.Count == 0) return;

        IsCommandPanelVisible = false;
        AvailableGangs.Clear();
        MovementTargets.Clear();
        QueuedCommands.Clear();
        SelectedGang = null;
        SelectedMovementTarget = null;
        CommandStatusMessage = null;
        RefreshCommandCommands();
    }

    private void RefreshRecruitmentCommands()
    {
        HireCommand.NotifyCanExecuteChanged();
        DeclineCommand.NotifyCanExecuteChanged();
    }

    private static string FormatAmount(int amount)
    {
        return amount > 0
            ? string.Format(CultureInfo.CurrentCulture, "+{0}", amount)
            : amount.ToString(CultureInfo.CurrentCulture);
    }

    private void RefreshCommandCommands()
    {
        QueueMoveCommand.NotifyCanExecuteChanged();
        QueueControlCommand.NotifyCanExecuteChanged();
        QueueInfluenceCommand.NotifyCanExecuteChanged();
        QueueChaosCommand.NotifyCanExecuteChanged();
        QueueResearchCommand.NotifyCanExecuteChanged();
        RemoveQueuedCommandCommand.NotifyCanExecuteChanged();
    }

    private bool IsCommandPhaseInteractive()
    {
        if (!_turnController.IsTurnActive) return false;

        if (_turnController.CurrentPhase != TurnPhase.Command) return false;

        if (!_gameSession.IsInitialized) return false;

        var state = _gameSession.GameState;
        return state.CurrentPlayer.Id == state.PrimaryPlayerId;
    }

    private void UpdateTurnEvents()
    {
        if (TurnEvents.Count > 0) TurnEvents.Clear();

        foreach (var entry in _eventLog.Events.OrderByDescending(e => e.TurnNumber).ThenByDescending(e => e.Timestamp))
            TurnEvents.Add(new TurnEventViewModel(entry));
    }

    partial void OnSelectedGangChanged(GangOptionViewModel? oldValue, GangOptionViewModel? newValue)
    {
        if (oldValue == newValue) return;

        if (!_gameSession.IsInitialized)
        {
            SelectedMovementTarget = null;
            return;
        }

        UpdateMovementTargets(_gameSession.GameState);
        RefreshCommandCommands();
        UpdatePreviews();
    }

    partial void OnSelectedMovementTargetChanged(SectorOptionViewModel? oldValue, SectorOptionViewModel? newValue)
    {
        if (oldValue == newValue) return;

        RefreshCommandCommands();
    }

    partial void OnIsCommandPanelVisibleChanged(bool oldValue, bool newValue)
    {
        if (!newValue) CommandStatusMessage = null;
    }

    private void EnsureSessionInitialised()
    {
        if (!_gameSession.IsInitialized) throw new InvalidOperationException("Game session has not been initialised.");
    }

    partial void OnSelectedSectorChanged(SectorOptionViewModel? oldValue, SectorOptionViewModel? newValue)
    {
        if (oldValue != newValue)
        {
            RefreshRecruitmentCommands();
            UpdatePreviews();
        }
    }

    private void UpdatePreviews()
    {
        if (!_gameSession.IsInitialized || SelectedGang is null)
        {
            ControlPreview = null;
            InfluencePreview = null;
            ResearchPreview = null;
            return;
        }

        var state = _gameSession.GameState;
        if (!state.Game.TryGetGang(SelectedGang.GangId, out var gang) || gang is null)
        {
            ControlPreview = null;
            InfluencePreview = null;
            ResearchPreview = null;
            return;
        }

        // Control preview uses the gang's current sector
        if (state.Game.TryGetSector(SelectedGang.SectorId, out var controlSector) && controlSector is not null)
        {
            var controlPower = gang.TotalStats.Control + gang.TotalStats.Strength;
            var incomePenalty = Math.Max(0, controlSector.Site?.Cash ?? 0);
            var supportPenalty = Math.Max(0, controlSector.Site?.Support ?? 0);
            var net = controlPower - incomePenalty - supportPenalty;
            ControlPreview = string.Format(CultureInfo.CurrentCulture,
                "Control Preview: Control {0} + Strength {1} - Cash {2} - Support {3} = Net {4} → {5}",
                gang.TotalStats.Control, gang.TotalStats.Strength, incomePenalty, supportPenalty, net,
                net >= 1 ? "Automatic Success" : "Automatic Failure");
        }
        else
        {
            ControlPreview = null;
        }

        // Influence preview uses SelectedSector if set, otherwise the gang's current sector
        var influenceSectorId = SelectedSector?.SectorId ?? SelectedGang.SectorId;
        if (state.Game.TryGetSector(influenceSectorId, out var influenceSector) && influenceSector is not null)
        {
            var influencePower = gang.TotalStats.Influence;
            var support = Math.Max(0, influenceSector.Site?.Support ?? 0);
            var security = Math.Max(0, influenceSector.Site?.Security ?? 0);
            var net = influencePower - (support + security);
            InfluencePreview = string.Format(CultureInfo.CurrentCulture,
                "Influence Preview: Influence {0} - (Support {1} + Security {2}) = Net {3} → {4}",
                influencePower, support, security, net, net >= 1 ? "Automatic Success" : "Automatic Failure");
        }
        else
        {
            InfluencePreview = null;
        }

        // Research preview for current player; target is typed project id or active one
        try
        {
            var preview = _researchService.BuildPreview(state, state.CurrentPlayer.Id);
            var target = string.IsNullOrWhiteSpace(ResearchProjectId)
                ? preview.ActiveProjectId ?? "(none)"
                : ResearchProjectId!;
            ResearchPreview = string.Format(CultureInfo.CurrentCulture, "Research Preview: {0} (Estimated +{1})",
                target, preview.EstimatedProgress);
        }
        catch
        {
            ResearchPreview = null;
        }
    }

    public sealed class ResearchSuggestionViewModel
    {
        public ResearchSuggestionViewModel(string name, int cost)
        {
            Name = name;
            Cost = cost;
            Display = string.Format(CultureInfo.CurrentCulture, "{0} ({1})", name, cost);
        }

        public string Name { get; }
        public int Cost { get; }
        public string Display { get; }
    }

    public sealed class FinanceCategoryViewModel
    {
        public FinanceCategoryViewModel(FinanceCategory category)
        {
            if (category is null) throw new ArgumentNullException(nameof(category));

            Type = category.Type;
            DisplayName = category.DisplayName;
            Amount = category.Amount;
            AmountDisplay = FormatAmount(category.Amount);
            IsExpense = category.IsExpense;
            IsIncome = category.IsIncome;
        }

        public FinanceCategoryType Type { get; }

        public string DisplayName { get; }

        public int Amount { get; }

        public string AmountDisplay { get; }

        public bool IsExpense { get; }

        public bool IsIncome { get; }
    }

    public sealed class FinanceSectorViewModel
    {
        public FinanceSectorViewModel(FinanceSectorProjection projection)
        {
            if (projection is null) throw new ArgumentNullException(nameof(projection));

            SectorId = projection.SectorId;
            DisplayName = projection.DisplayName;
            NetChange = projection.NetChange;
            NetChangeDisplay = FormatAmount(projection.NetChange);
            Categories = projection.Categories.Select(category => new FinanceCategoryViewModel(category)).ToList();
        }

        public string SectorId { get; }

        public string DisplayName { get; }

        public int NetChange { get; }

        public string NetChangeDisplay { get; }

        public IReadOnlyList<FinanceCategoryViewModel> Categories { get; }
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

            return string.Format(CultureInfo.CurrentCulture, "Turn {0}: [{1}] {2}", entry.TurnNumber, phaseText,
                entry.Description);
        }
    }

    public sealed partial class RecruitmentOptionViewModel : ObservableObject
    {
        [ObservableProperty] private string _gangName = string.Empty;

        [ObservableProperty] private int _hiringCost;

        [ObservableProperty] private Guid _optionId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StatusDisplay))]
        [NotifyPropertyChangedFor(nameof(IsAvailable))]
        [NotifyPropertyChangedFor(nameof(IsDeclined))]
        [NotifyPropertyChangedFor(nameof(IsHired))]
        private RecruitmentOptionState _state;

        [ObservableProperty] private int _upkeepCost;

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

    public sealed class GangOptionViewModel
    {
        public GangOptionViewModel(Guid gangId, string name, string sectorId)
        {
            GangId = gangId;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            SectorId = sectorId ?? throw new ArgumentNullException(nameof(sectorId));
        }

        public Guid GangId { get; }

        public string Name { get; }

        public string SectorId { get; }

        public string DisplayName => string.Format(CultureInfo.CurrentCulture, "{0} ({1})", Name, SectorId);
    }

    public sealed class QueuedCommandViewModel
    {
        public QueuedCommandViewModel(PlayerCommandSnapshot snapshot)
        {
            if (snapshot is null) throw new ArgumentNullException(nameof(snapshot));

            CommandId = snapshot.CommandId;
            GangId = snapshot.GangId;
            GangName = snapshot.GangName;
            Kind = snapshot.Kind;
            Phase = snapshot.Phase;
            SourceSectorId = snapshot.SourceSectorId;
            TargetSectorId = snapshot.TargetSectorId;
            ProjectedChaos = snapshot.ProjectedChaos;
            Description = snapshot.Description ?? BuildDescription(snapshot);
        }

        public Guid CommandId { get; }

        public Guid GangId { get; }

        public string GangName { get; }

        public PlayerCommandKind Kind { get; }

        public CommandPhase Phase { get; }

        public string? SourceSectorId { get; }

        public string? TargetSectorId { get; }

        public int ProjectedChaos { get; }

        public string Description { get; }

        public string Summary => string.Format(
            CultureInfo.CurrentCulture,
            "{0} – {1}: {2}",
            GangName,
            Phase,
            Description);

        private static string BuildDescription(PlayerCommandSnapshot snapshot)
        {
            return snapshot.Kind switch
            {
                PlayerCommandKind.Move => string.Format(
                    CultureInfo.CurrentCulture,
                    "Move from {0} to {1}",
                    snapshot.SourceSectorId,
                    snapshot.TargetSectorId),
                PlayerCommandKind.Influence => string.Format(
                    CultureInfo.CurrentCulture,
                    "Influence {0}",
                    snapshot.TargetSectorId),
                PlayerCommandKind.Control => string.Format(
                    CultureInfo.CurrentCulture,
                    "Control {0}",
                    snapshot.TargetSectorId),
                PlayerCommandKind.Chaos => string.Format(
                    CultureInfo.CurrentCulture,
                    "Chaos {0} in {1}",
                    snapshot.ProjectedChaos,
                    snapshot.TargetSectorId),
                _ => snapshot.Kind.ToString()
            };
        }
    }
}