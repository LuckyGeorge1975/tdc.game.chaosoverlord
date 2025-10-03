using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChaosOverlords.App.ViewModels.Sections;

public sealed class TurnEventsSectionViewModel : ObservableObject, IDisposable
{
    private readonly Action? _openLogsAction;

    // UX toggle: when true, the view should auto-scroll to the newest event as they arrive.
    private bool _autoScrollToLatest = true;

    public TurnEventsSectionViewModel(ObservableCollection<TurnViewModel.TurnEventViewModel> events,
        Action? openLogsAction = null)
    {
        Events = events ?? throw new ArgumentNullException(nameof(events));
        _openLogsAction = openLogsAction;
        Events.CollectionChanged += OnCollectionChanged;
    }

    public ObservableCollection<TurnViewModel.TurnEventViewModel> Events { get; }

    public bool HasEvents => Events.Count > 0;

    public bool AutoScrollToLatest
    {
        get => _autoScrollToLatest;
        set => SetProperty(ref _autoScrollToLatest, value);
    }

    public IRelayCommand OpenLogsFolderCommand => new RelayCommand(() => _openLogsAction?.Invoke());

    public void Dispose()
    {
        Events.CollectionChanged -= OnCollectionChanged;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasEvents));
    }
}