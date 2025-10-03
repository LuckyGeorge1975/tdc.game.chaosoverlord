using System.Collections.ObjectModel;
using System.ComponentModel;
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

    public ObservableCollection<TurnViewModel.SectorOptionViewModel> ControlledSectors => _owner.ControlledSectors;

    public ObservableCollection<TurnViewModel.QueuedCommandViewModel> QueuedCommands => _owner.QueuedCommands;

    public bool IsVisible => _owner.IsCommandPanelVisible;

    public string? StatusMessage => _owner.CommandStatusMessage;

    public bool HasStatusMessage => _owner.HasCommandStatusMessage;

    public string? ControlPreview => _owner.ControlPreview;
    public bool HasControlPreview => _owner.HasControlPreview;
    public string? InfluencePreview => _owner.InfluencePreview;
    public bool HasInfluencePreview => _owner.HasInfluencePreview;
    public string? ResearchPreview => _owner.ResearchPreview;
    public bool HasResearchPreview => _owner.HasResearchPreview;

    public ObservableCollection<TurnViewModel.ResearchSuggestionViewModel> ResearchSuggestions =>
        _owner.ResearchSuggestions;

    public TurnViewModel.ResearchSuggestionViewModel? SelectedResearchSuggestion
    {
        get => null;
        set
        {
            if (value is not null)
            {
                _owner.ResearchProjectId = value.Name;
                OnPropertyChanged(nameof(ResearchProjectId));
            }
        }
    }

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

    public string? ResearchProjectId
    {
        get => _owner.ResearchProjectId;
        set
        {
            if (!Equals(_owner.ResearchProjectId, value))
            {
                _owner.ResearchProjectId = value;
                OnPropertyChanged();
            }
        }
    }

    // Influence target sector (reuses TurnViewModel.SelectedSector)
    public TurnViewModel.SectorOptionViewModel? SelectedInfluenceTarget
    {
        get => _owner.SelectedSector;
        set
        {
            if (!Equals(_owner.SelectedSector, value))
            {
                _owner.SelectedSector = value;
                OnPropertyChanged();
            }
        }
    }

    public IRelayCommand QueueMoveCommand => _owner.QueueMoveCommand;

    public IRelayCommand QueueControlCommand => _owner.QueueControlCommand;

    public IRelayCommand QueueInfluenceCommand => _owner.QueueInfluenceCommand;

    public IRelayCommand QueueChaosCommand => _owner.QueueChaosCommand;
    public IRelayCommand QueueResearchCommand => _owner.QueueResearchCommand;

    public IRelayCommand<TurnViewModel.QueuedCommandViewModel> RemoveQueuedCommandCommand =>
        _owner.RemoveQueuedCommandCommand;

    public void Dispose()
    {
        _owner.PropertyChanged -= OnOwnerPropertyChanged;
    }

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
            case nameof(TurnViewModel.ControlPreview):
                OnPropertyChanged(nameof(ControlPreview));
                OnPropertyChanged(nameof(HasControlPreview));
                break;
            case nameof(TurnViewModel.InfluencePreview):
                OnPropertyChanged(nameof(InfluencePreview));
                OnPropertyChanged(nameof(HasInfluencePreview));
                break;
            case nameof(TurnViewModel.ResearchPreview):
                OnPropertyChanged(nameof(ResearchPreview));
                OnPropertyChanged(nameof(HasResearchPreview));
                break;
            case nameof(TurnViewModel.ResearchProjectId):
                OnPropertyChanged(nameof(ResearchProjectId));
                break;
            case nameof(TurnViewModel.SelectedGang):
                OnPropertyChanged(nameof(SelectedGang));
                break;
            case nameof(TurnViewModel.SelectedMovementTarget):
                OnPropertyChanged(nameof(SelectedMovementTarget));
                break;
            case nameof(TurnViewModel.SelectedSector):
                OnPropertyChanged(nameof(SelectedInfluenceTarget));
                break;
            case nameof(TurnViewModel.ControlledSectors):
                OnPropertyChanged(nameof(ControlledSectors));
                break;
        }
    }
}