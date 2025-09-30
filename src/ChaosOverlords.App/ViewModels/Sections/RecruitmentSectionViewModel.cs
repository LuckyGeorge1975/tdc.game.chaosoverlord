using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChaosOverlords.App.ViewModels.Sections;

public sealed class RecruitmentSectionViewModel : ObservableObject, IDisposable
{
    private readonly TurnViewModel _owner;

    public RecruitmentSectionViewModel(TurnViewModel owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _owner.PropertyChanged += OnOwnerPropertyChanged;
    }

    public ObservableCollection<TurnViewModel.RecruitmentOptionViewModel> RecruitmentOptions => _owner.RecruitmentOptions;

    public ObservableCollection<TurnViewModel.SectorOptionViewModel> ControlledSectors => _owner.ControlledSectors;

    public bool IsVisible => _owner.IsRecruitmentPanelVisible;

    public string? StatusMessage => _owner.RecruitmentStatusMessage;

    public bool HasStatusMessage => _owner.HasRecruitmentStatusMessage;

    public TurnViewModel.SectorOptionViewModel? SelectedSector
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

    public IRelayCommand<TurnViewModel.RecruitmentOptionViewModel> HireCommand => _owner.HireCommand;

    public IRelayCommand<TurnViewModel.RecruitmentOptionViewModel> DeclineCommand => _owner.DeclineCommand;

    public void Dispose() => _owner.PropertyChanged -= OnOwnerPropertyChanged;

    private void OnOwnerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(TurnViewModel.IsRecruitmentPanelVisible):
                OnPropertyChanged(nameof(IsVisible));
                break;
            case nameof(TurnViewModel.RecruitmentStatusMessage):
                OnPropertyChanged(nameof(StatusMessage));
                break;
            case nameof(TurnViewModel.HasRecruitmentStatusMessage):
                OnPropertyChanged(nameof(HasStatusMessage));
                break;
            case nameof(TurnViewModel.SelectedSector):
                OnPropertyChanged(nameof(SelectedSector));
                break;
        }
    }
}
