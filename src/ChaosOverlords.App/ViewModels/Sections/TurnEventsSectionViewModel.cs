using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChaosOverlords.App.ViewModels.Sections;

public sealed class TurnEventsSectionViewModel : ObservableObject, IDisposable
{
    private readonly ObservableCollection<TurnViewModel.TurnEventViewModel> _events;
    private readonly Action? _openLogsAction;

    public TurnEventsSectionViewModel(ObservableCollection<TurnViewModel.TurnEventViewModel> events, Action? openLogsAction = null)
    {
        _events = events ?? throw new ArgumentNullException(nameof(events));
        _openLogsAction = openLogsAction;
        _events.CollectionChanged += OnCollectionChanged;
    }

    public ObservableCollection<TurnViewModel.TurnEventViewModel> Events => _events;

    public bool HasEvents => _events.Count > 0;

    // UX toggle: when true, the view should auto-scroll to the newest event as they arrive.
    private bool _autoScrollToLatest = true;
    public bool AutoScrollToLatest
    {
        get => _autoScrollToLatest;
        set => SetProperty(ref _autoScrollToLatest, value);
    }

    public CommunityToolkit.Mvvm.Input.IRelayCommand OpenLogsFolderCommand => new CommunityToolkit.Mvvm.Input.RelayCommand(() => _openLogsAction?.Invoke());

    public void Dispose() => _events.CollectionChanged -= OnCollectionChanged;

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasEvents));
    }
}
