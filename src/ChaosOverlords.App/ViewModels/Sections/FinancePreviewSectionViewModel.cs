using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChaosOverlords.App.ViewModels.Sections;

public sealed class FinancePreviewSectionViewModel : ObservableObject, IDisposable
{
    private readonly TurnViewModel _owner;

    public FinancePreviewSectionViewModel(TurnViewModel owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _owner.PropertyChanged += OnOwnerPropertyChanged;
    }

    public ObservableCollection<TurnViewModel.FinanceCategoryViewModel> CityFinanceCategories => _owner.CityFinanceCategories;

    public ObservableCollection<TurnViewModel.FinanceSectorViewModel> SectorFinance => _owner.SectorFinance;

    public string Heading => _owner.FinancePreviewHeading;

    public string CityNetChangeDisplay => _owner.CityNetChangeDisplay;

    public bool IsVisible => _owner.IsFinancePreviewVisible;

    public void Dispose() => _owner.PropertyChanged -= OnOwnerPropertyChanged;

    private void OnOwnerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(TurnViewModel.FinancePreviewHeading):
                OnPropertyChanged(nameof(Heading));
                break;
            case nameof(TurnViewModel.CityNetChangeDisplay):
                OnPropertyChanged(nameof(CityNetChangeDisplay));
                break;
            case nameof(TurnViewModel.IsFinancePreviewVisible):
                OnPropertyChanged(nameof(IsVisible));
                break;
        }
    }
}
