using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChaosOverlords.App.ViewModels.Sections;

public sealed class TurnEventsSectionViewModel : ObservableObject, IDisposable
{
    private readonly ObservableCollection<TurnViewModel.TurnEventViewModel> _events;

    public TurnEventsSectionViewModel(ObservableCollection<TurnViewModel.TurnEventViewModel> events)
    {
        _events = events ?? throw new ArgumentNullException(nameof(events));
        _events.CollectionChanged += OnCollectionChanged;
    }

    public ObservableCollection<TurnViewModel.TurnEventViewModel> Events => _events;

    public bool HasEvents => _events.Count > 0;

    public void Dispose() => _events.CollectionChanged -= OnCollectionChanged;

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasEvents));
    }
}
