using System.Collections.ObjectModel;
using System.Globalization;
using ChaosOverlords.App.Messaging;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;
using ChaosOverlords.Core.Services.Messaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChaosOverlords.App.ViewModels;

/// <summary>
///     Projects the current city map into UI-friendly sector tiles.
/// </summary>
public sealed class MapViewModel : ViewModelBase, IDisposable
{
    private readonly IGameSession _gameSession;
    private readonly IMessageHub _messageHub;
    private readonly Dictionary<string, SectorTileViewModel> _tilesById;
    private readonly IDisposable _turnSubscription;

    public MapViewModel(IGameSession gameSession, IDataService dataService, IMessageHub messageHub)
    {
        _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        if (dataService is null) throw new ArgumentNullException(nameof(dataService));

        _messageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));

        var configuration = dataService.GetSectorConfigurationAsync().GetAwaiter().GetResult()
                            ?? throw new InvalidOperationException("Sector configuration could not be loaded.");
        var siteList = dataService.GetSitesAsync().GetAwaiter().GetResult()
                       ?? throw new InvalidOperationException("Site data could not be loaded.");

        var siteLookup = siteList.ToDictionary(static s => s.Name, static s => s, StringComparer.OrdinalIgnoreCase);

        var (tiles, lookup) = CreateTiles(configuration, siteLookup);
        Tiles = new ObservableCollection<SectorTileViewModel>(tiles);
        _tilesById = lookup;

        RefreshFromGameState();

        _turnSubscription = _messageHub.Subscribe<TurnSummaryChangedMessage>(_ => RefreshFromGameState());
    }

    /// <summary>
    ///     Tiles rendered on the map surface, ordered by row and column.
    /// </summary>
    public ObservableCollection<SectorTileViewModel> Tiles { get; }

    public void Dispose()
    {
        _turnSubscription.Dispose();
    }

    private void RefreshFromGameState()
    {
        var state = _gameSession.GameState;
        var game = state?.Game;
        if (game is null)
        {
            foreach (var tile in Tiles) tile.ClearDynamicState();

            return;
        }

        var playerNames = game.Players.Values.ToDictionary(static p => p.Id, static p => p.Name);
        var playerColors = game.Players.Values.ToDictionary(static p => p.Id, p => p.Color.ToString());

        foreach (var (sectorId, tile) in _tilesById)
            if (game.Sectors.TryGetValue(sectorId, out var sector))
                tile.Apply(sector, playerNames, playerColors);
            else
                tile.ClearDynamicState();
    }

    private static (IReadOnlyList<SectorTileViewModel> Tiles, Dictionary<string, SectorTileViewModel> Lookup)
        CreateTiles(
            SectorConfigurationData configuration,
            IReadOnlyDictionary<string, SiteData> siteLookup)
    {
        if (configuration.Sectors is null || configuration.Sectors.Count == 0)
            throw new InvalidOperationException("Sector configuration must define at least one sector entry.");

        var tiles = new List<SectorTileViewModel>(configuration.Sectors.Count);
        var lookup = new Dictionary<string, SectorTileViewModel>(StringComparer.OrdinalIgnoreCase);

        foreach (var sector in configuration.Sectors)
        {
            if (string.IsNullOrWhiteSpace(sector.Id)) continue;

            var (row, column) = ParseCoordinates(sector.Id);
            SiteData? defaultSite = null;
            if (!string.IsNullOrWhiteSpace(sector.SiteName) &&
                siteLookup.TryGetValue(sector.SiteName, out var siteData)) defaultSite = siteData;

            var tile = new SectorTileViewModel(sector.Id, row, column, defaultSite);
            tiles.Add(tile);
            lookup[sector.Id] = tile;
        }

        tiles.Sort(static (left, right) =>
        {
            var comparison = left.Row.CompareTo(right.Row);
            return comparison != 0 ? comparison : left.Column.CompareTo(right.Column);
        });

        return (tiles, lookup);
    }

    private static (int Row, int Column) ParseCoordinates(string sectorId)
    {
        if (string.IsNullOrWhiteSpace(sectorId) || sectorId.Length < 2)
            throw new InvalidOperationException(
                "Sector identifiers must contain a row letter followed by a column number.");

        var rowLetter = char.ToUpperInvariant(sectorId[0]);
        if (rowLetter < 'A' || rowLetter > 'Z')
            throw new InvalidOperationException($"Sector '{sectorId}' contains an invalid row identifier.");

        if (!int.TryParse(sectorId[1..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var column) ||
            column < 1)
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                "Sector '{0}' contains an invalid column identifier.", sectorId));

        return (rowLetter - 'A', column - 1);
    }

    /// <summary>
    ///     Presentation model for an individual sector tile.
    /// </summary>
    public sealed class SectorTileViewModel : ObservableObject
    {
        private static readonly (string Label, Func<SiteData, int> Extractor)[] SiteStatExtractors =
        {
            ("Cash", static site => site.Cash),
            ("Tolerance", static site => site.Tolerance),
            ("Support", static site => site.Support),
            ("Resistance", static site => site.Resistance),
            ("Chaos", static site => site.Chaos),
            ("Control", static site => site.Control),
            ("Combat", static site => site.Combat),
            ("Defense", static site => site.Defense),
            ("Strength", static site => site.Strength),
            ("Stealth", static site => site.Stealth),
            ("Detect", static site => site.Detect),
            ("Influence", static site => site.Influence),
            ("Research", static site => site.Research),
            ("Heal", static site => site.Heal),
            ("Blade Melee", static site => site.BladeMelee),
            ("Ranged", static site => site.Ranged),
            ("Fighting", static site => site.Fighting),
            ("Martial Arts", static site => site.MartialArts),
            ("Security", static site => site.Security),
            ("Equipment Discount %", static site => site.EquipmentDiscountPercent),
            ("Enables Tech Level", static site => site.EnablesResearchThroughTechLevel)
        };

        private readonly int _defaultIncome;
        private readonly SiteData? _defaultSiteData;
        private readonly string? _defaultSiteName;
        private readonly int _defaultTolerance;
        private string? _activeSiteName;
        private int _chaosProjection;

        private string? _controllerName;
        private int? _currentIncome;
        private SiteData? _currentSiteData;
        private int? _currentTolerance;
        private int _gangCount;
        private bool _isControlled;

        private string? _ownerColor;

        internal SectorTileViewModel(string sectorId, int row, int column, SiteData? defaultSite)
        {
            SectorId = sectorId;
            Row = row;
            Column = column;

            _defaultSiteData = defaultSite;
            _defaultSiteName = defaultSite?.Name;
            _defaultIncome = defaultSite?.Cash ?? 0;
            _defaultTolerance = defaultSite?.Tolerance ?? 0;

            _currentSiteData = defaultSite;
            _activeSiteName = _defaultSiteName;
            _currentIncome = defaultSite?.Cash;
            _currentTolerance = defaultSite?.Tolerance;
        }

        public string SectorId { get; }

        public int Row { get; }

        public int Column { get; }

        public string Label => SectorId;

        public string OwnerDisplay => _controllerName ?? "Neutral";

        public string? OwnerColor
        {
            get => _ownerColor;
            private set => SetProperty(ref _ownerColor, value);
        }

        public bool IsControlled
        {
            get => _isControlled;
            private set => SetProperty(ref _isControlled, value);
        }

        public string SiteDisplay => _activeSiteName is { Length: > 0 }
            ? string.Format(CultureInfo.CurrentCulture, "Site: {0}", _activeSiteName)
            : "Site: Unassigned";

        public string SiteTooltip => BuildSiteTooltip(_currentSiteData ?? _defaultSiteData, OwnerDisplay, _gangCount,
            _chaosProjection);

        public string IncomeDisplay => FormatAmount(_currentIncome ?? _defaultIncome);

        public string ToleranceDisplay =>
            string.Format(CultureInfo.CurrentCulture, "Tol {0}", _currentTolerance ?? _defaultTolerance);

        public string ChaosDisplay => _chaosProjection == 0
            ? "Chaos 0"
            : string.Format(CultureInfo.CurrentCulture, "Chaos {0}", _chaosProjection);

        public string GangDisplay => _gangCount == 0
            ? "No Gangs"
            : string.Format(CultureInfo.CurrentCulture, _gangCount == 1 ? "{0} Gang" : "{0} Gangs", _gangCount);

        internal void Apply(Sector sector, IReadOnlyDictionary<Guid, string> playerNames,
            IReadOnlyDictionary<Guid, string> playerColors)
        {
            _controllerName = sector.ControllingPlayerId.HasValue &&
                              playerNames.TryGetValue(sector.ControllingPlayerId.Value, out var name)
                ? name
                : null;
            _gangCount = sector.GangIds.Count;
            _chaosProjection = sector.ProjectedChaos;
            _currentSiteData = sector.Site;
            _activeSiteName = sector.Site.Name;
            _currentIncome = sector.Site.Cash;
            _currentTolerance = sector.Site.Tolerance;
            IsControlled = sector.ControllingPlayerId.HasValue;

            // Owner color comes from player color enum name; UI can map it to a brush.
            OwnerColor = sector.ControllingPlayerId.HasValue &&
                         playerColors.TryGetValue(sector.ControllingPlayerId.Value, out var color)
                ? color
                : null;

            OnPropertyChanged(nameof(OwnerDisplay));
            OnPropertyChanged(nameof(IsControlled));
            OnPropertyChanged(nameof(GangDisplay));
            OnPropertyChanged(nameof(ChaosDisplay));
            OnPropertyChanged(nameof(SiteDisplay));
            OnPropertyChanged(nameof(IncomeDisplay));
            OnPropertyChanged(nameof(ToleranceDisplay));
            OnPropertyChanged(nameof(SiteTooltip));
            OnPropertyChanged(nameof(OwnerColor));
        }

        internal void ClearDynamicState()
        {
            _controllerName = null;
            _gangCount = 0;
            _chaosProjection = 0;
            _currentSiteData = _defaultSiteData;
            _activeSiteName = _defaultSiteName;
            _currentIncome = _defaultIncome;
            _currentTolerance = _defaultTolerance;
            IsControlled = false;
            OwnerColor = null;

            OnPropertyChanged(nameof(OwnerDisplay));
            OnPropertyChanged(nameof(IsControlled));
            OnPropertyChanged(nameof(GangDisplay));
            OnPropertyChanged(nameof(ChaosDisplay));
            OnPropertyChanged(nameof(SiteDisplay));
            OnPropertyChanged(nameof(IncomeDisplay));
            OnPropertyChanged(nameof(ToleranceDisplay));
            OnPropertyChanged(nameof(SiteTooltip));
            OnPropertyChanged(nameof(OwnerColor));
        }

        private static string FormatAmount(int amount)
        {
            var prefix = amount >= 0 ? "+" : string.Empty;
            return string.Format(CultureInfo.CurrentCulture, "Cash {0}{1}", prefix, amount);
        }

        private static string BuildSiteTooltip(SiteData? site, string ownerDisplay, int gangCount, int chaosProjection)
        {
            var lines = new List<string>
            {
                string.Format(CultureInfo.CurrentCulture, "Site: {0}", site?.Name ?? "Unassigned"),
                string.Format(CultureInfo.CurrentCulture, "Owner: {0}", ownerDisplay),
                string.Format(CultureInfo.CurrentCulture, "Gangs: {0}", gangCount),
                string.Format(CultureInfo.CurrentCulture, "Chaos Projection: {0}", chaosProjection)
            };

            if (site is null)
            {
                lines.Add("No site data available.");
                return string.Join(Environment.NewLine, lines);
            }

            var statLines = new List<string>();
            foreach (var (label, extractor) in SiteStatExtractors)
            {
                var value = extractor(site);
                if (value != 0) statLines.Add(string.Format(CultureInfo.CurrentCulture, "{0}: {1}", label, value));
            }

            if (statLines.Count == 0) statLines.Add("No site modifiers.");

            lines.AddRange(statLines);
            return string.Join(Environment.NewLine, lines);
        }
    }
}