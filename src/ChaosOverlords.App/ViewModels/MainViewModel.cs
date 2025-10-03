namespace ChaosOverlords.App.ViewModels;

/// <summary>
///     Application shell view model that aggregates the primary screens.
/// </summary>
public sealed class MainViewModel(MapViewModel mapViewModel, TurnViewModel turnViewModel) : ViewModelBase
{
    /// <summary>
    ///     View model representing the city map.
    /// </summary>
    public MapViewModel Map { get; } = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));

    /// <summary>
    ///     View model managing the active turn state machine.
    /// </summary>
    public TurnViewModel Turn { get; } = turnViewModel ?? throw new ArgumentNullException(nameof(turnViewModel));
}