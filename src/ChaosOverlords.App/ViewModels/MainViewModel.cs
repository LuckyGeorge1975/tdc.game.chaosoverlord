namespace ChaosOverlords.App.ViewModels;

/// <summary>
/// Application shell view model that aggregates the primary screens.
/// </summary>
public sealed class MainViewModel : ViewModelBase
{
    public MainViewModel(MapViewModel mapViewModel, TurnViewModel turnViewModel)
    {
        Map = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));
        Turn = turnViewModel ?? throw new ArgumentNullException(nameof(turnViewModel));
    }

    /// <summary>
    /// View model representing the city map.
    /// </summary>
    public MapViewModel Map { get; }

    /// <summary>
    /// View model managing the active turn state machine.
    /// </summary>
    public TurnViewModel Turn { get; }
}
