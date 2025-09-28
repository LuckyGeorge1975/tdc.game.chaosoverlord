using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChaosOverlords.App.ViewModels;

/// <summary>
/// Placeholder map view model that exposes an 8x8 grid.
/// </summary>
public sealed partial class MapViewModel : ViewModelBase
{
    public const int DefaultSize = 8;

    public MapViewModel()
    {
        Tiles = Enumerable.Range(0, DefaultSize * DefaultSize)
            .Select(index => new MapTileViewModel(index / DefaultSize, index % DefaultSize))
            .ToArray();
    }

    /// <summary>
    /// Placeholder tiles rendered in the UI.
    /// </summary>
    public IReadOnlyList<MapTileViewModel> Tiles { get; }

    /// <summary>
    /// Simplified map tile data structure to display sector coordinates.
    /// </summary>
    public sealed partial class MapTileViewModel : ObservableObject
    {
        public MapTileViewModel(int row, int column)
        {
            Row = row;
            Column = column;
            Label = $"{(char)('A' + row)}{column + 1}";
        }

        public int Row { get; }

        public int Column { get; }

        public string Label { get; }
    }
}
