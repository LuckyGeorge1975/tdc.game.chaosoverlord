namespace ChaosOverlords.App.ViewModels;

/// <summary>
/// Application shell view model that hosts the currently active screen.
/// </summary>
public sealed class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel;

    public MainViewModel(MapViewModel mapViewModel)
    {
        _currentViewModel = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));
    }

    /// <summary>
    /// Gets or sets the view model currently displayed by the main window.
    /// </summary>
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value ?? throw new ArgumentNullException(nameof(value)));
    }

    /// <summary>
    /// Helper used by navigation routines to switch the displayed view model.
    /// </summary>
    public void SetCurrentViewModel(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }
}
