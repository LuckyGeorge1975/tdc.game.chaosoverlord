using System.Globalization;
using ChaosOverlords.App.Messaging;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Services.Messaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChaosOverlords.App.ViewModels.Sections;

public sealed partial class TurnManagementSectionViewModel : ObservableObject, IDisposable
{
    private readonly IDisposable _subscription;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveCommandPhaseDisplay))]
    [NotifyPropertyChangedFor(nameof(HasActiveCommandPhase))]
    private CommandPhase? _activeCommandPhase;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CurrentPhaseDisplay))]
    private TurnPhase _currentPhase;

    [ObservableProperty] private bool _isTurnActive;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(TurnCounterDisplay))]
    private int _turnNumber;

    public TurnManagementSectionViewModel(
        IMessageHub messageHub,
        IRelayCommand startTurnCommand,
        IRelayCommand advancePhaseCommand,
        IRelayCommand endTurnCommand)
    {
        StartTurnCommand = startTurnCommand ?? throw new ArgumentNullException(nameof(startTurnCommand));
        AdvancePhaseCommand = advancePhaseCommand ?? throw new ArgumentNullException(nameof(advancePhaseCommand));
        EndTurnCommand = endTurnCommand ?? throw new ArgumentNullException(nameof(endTurnCommand));

        if (messageHub is null) throw new ArgumentNullException(nameof(messageHub));

        _subscription = messageHub.Subscribe<TurnSummaryChangedMessage>(OnTurnSummaryChanged);
    }

    public IRelayCommand StartTurnCommand { get; }

    public IRelayCommand AdvancePhaseCommand { get; }

    public IRelayCommand EndTurnCommand { get; }

    public string TurnCounterDisplay => TurnNumber <= 0
        ? "Turn 0"
        : string.Format(CultureInfo.CurrentCulture, "Turn {0}", TurnNumber);

    public string CurrentPhaseDisplay => CurrentPhase.ToString();

    public string? ActiveCommandPhaseDisplay => ActiveCommandPhase?.ToString();

    public bool HasActiveCommandPhase => ActiveCommandPhase.HasValue;

    public void Dispose()
    {
        _subscription.Dispose();
    }

    private void OnTurnSummaryChanged(TurnSummaryChangedMessage message)
    {
        TurnNumber = message.TurnNumber;
        CurrentPhase = message.CurrentPhase;
        ActiveCommandPhase = message.ActiveCommandPhase;
        IsTurnActive = message.IsTurnActive;
    }
}