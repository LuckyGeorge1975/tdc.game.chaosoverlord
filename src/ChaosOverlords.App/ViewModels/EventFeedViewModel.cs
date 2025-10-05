using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using ChaosOverlords.App.Messaging;
using ChaosOverlords.Core.Domain.Game.Events;
using ChaosOverlords.Core.Services.Messaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChaosOverlords.App.ViewModels;

/// <summary>
///     View model for the event feed panel displaying turn events.
/// </summary>
public sealed partial class EventFeedViewModel : ViewModelBase, IDisposable
{
    private readonly ITurnEventLog _eventLog;
    private readonly TurnViewModel _turnViewModel;
    private readonly IMessageHub _messageHub;
    private readonly IDisposable _subscription;

    /// <summary>
    ///     Observable collection of turn events for binding.
    /// </summary>
    public ObservableCollection<TurnEvent> Events { get; } = new();

    /// <summary>
    ///     Currently selected event.
    /// </summary>
    [ObservableProperty]
    private TurnEvent? _selectedEvent;

    public EventFeedViewModel(ITurnEventLog eventLog, TurnViewModel turnViewModel, IMessageHub messageHub)
    {
        _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
        _turnViewModel = turnViewModel ?? throw new ArgumentNullException(nameof(turnViewModel));
        _messageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
        _subscription = _messageHub.Subscribe<TurnEventsChangedMessage>(_ => SyncEvents());
        SyncEvents();
    }

    private void SyncEvents()
    {
        Events.Clear();
        foreach (var evt in _eventLog.Events)
        {
            Events.Add(evt);
        }
    }

    /// <summary>
    ///     Command to handle clicking on an event.
    /// </summary>
    [RelayCommand]
    private void SelectEvent(TurnEvent? selectedEvent)
    {
        if (selectedEvent is null) return;

        // Try to extract sector id from description
        var match = Regex.Match(selectedEvent.Description, @"Sector (\d+)", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var sectorId))
        {
            var sector = _turnViewModel.ControlledSectors.FirstOrDefault(s => s.SectorId == sectorId.ToString());
            if (sector is not null)
            {
                _turnViewModel.SelectedSector = sector;
            }
        }
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}
