using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ChaosOverlords.App.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChaosOverlords.App.ViewModels.Sections;

public sealed class CommandQueueSectionViewModel : ObservableObject, IDisposable
{
    private readonly TurnViewModel _owner;

    public CommandQueueSectionViewModel(TurnViewModel owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _owner.PropertyChanged += OnOwnerPropertyChanged;
    }

    public ObservableCollection<TurnViewModel.GangOptionViewModel> AvailableGangs => _owner.AvailableGangs;

    public ObservableCollection<TurnViewModel.SectorOptionViewModel> MovementTargets => _owner.MovementTargets;

    public ObservableCollection<TurnViewModel.QueuedCommandViewModel> QueuedCommands => _owner.QueuedCommands;

    public bool IsVisible => _owner.IsCommandPanelVisible;

    public string? StatusMessage => _owner.CommandStatusMessage;

    public bool HasStatusMessage => _owner.HasCommandStatusMessage;

    public TurnViewModel.GangOptionViewModel? SelectedGang
    {
        get => _owner.SelectedGang;
        set
        {
            if (!Equals(_owner.SelectedGang, value))
            {
                _owner.SelectedGang = value;
                OnPropertyChanged();
            }
        }
    }

    public TurnViewModel.SectorOptionViewModel? SelectedMovementTarget
    {
        get => _owner.SelectedMovementTarget;
        set
        {
            if (!Equals(_owner.SelectedMovementTarget, value))
            {
                _owner.SelectedMovementTarget = value;
                OnPropertyChanged();
            }
        }
    }

    public IRelayCommand QueueMoveCommand => _owner.QueueMoveCommand;

    public IRelayCommand QueueControlCommand => _owner.QueueControlCommand;

    public IRelayCommand QueueChaosCommand => _owner.QueueChaosCommand;

    public IRelayCommand<TurnViewModel.QueuedCommandViewModel> RemoveQueuedCommandCommand => _owner.RemoveQueuedCommandCommand;

    public void Dispose() => _owner.PropertyChanged -= OnOwnerPropertyChanged;

    private void OnOwnerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(TurnViewModel.IsCommandPanelVisible):
                OnPropertyChanged(nameof(IsVisible));
                break;
            case nameof(TurnViewModel.CommandStatusMessage):
                OnPropertyChanged(nameof(StatusMessage));
                break;
            case nameof(TurnViewModel.HasCommandStatusMessage):
                OnPropertyChanged(nameof(HasStatusMessage));
                break;
            case nameof(TurnViewModel.SelectedGang):
                OnPropertyChanged(nameof(SelectedGang));
                break;
            case nameof(TurnViewModel.SelectedMovementTarget):
                OnPropertyChanged(nameof(SelectedMovementTarget));
                break;
        }
    }
}
