using System.Globalization;
using System.Security.Cryptography;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Core.Services;

/// <summary>
///     Default implementation that wires reference data into runtime game state for freshly created campaigns.
/// </summary>
public sealed class ScenarioService(IDataService dataService, IRngService rngService) : IScenarioService
{
    private readonly IDataService _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
    private readonly IRngService _rngService = rngService ?? throw new ArgumentNullException(nameof(rngService));

    public async Task<GameState> CreateNewGameAsync(ScenarioConfig config,
        CancellationToken cancellationToken = default)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));

        ValidateConfig(config);

        var seed = config.Seed ?? RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
        if (seed == 0)
            // Seed value 0 is reserved or may cause issues with some RNG implementations, so we replace it with 1.
            seed = 1;

        _rngService.Reset(seed);

        var gangLookup = await BuildGangLookupAsync(cancellationToken).ConfigureAwait(false);
        var siteLookup = await BuildSiteLookupAsync(cancellationToken).ConfigureAwait(false);
        var sectorMetadata = await BuildSectorMetadataAsync(cancellationToken).ConfigureAwait(false);
        var siteAllocator = new SiteAllocator(siteLookup, _rngService);

        var players = new List<IPlayer>();
        var sectors = new Dictionary<string, Sector>(StringComparer.OrdinalIgnoreCase);
        var gangs = new List<Gang>();

        var palette = new[]
        {
            PlayerColor.Red, PlayerColor.Blue, PlayerColor.Green, PlayerColor.Yellow, PlayerColor.Purple,
            PlayerColor.Orange, PlayerColor.Cyan, PlayerColor.Magenta
        };
        var colorIndex = 0;

        foreach (var slot in config.Players)
        {
            var player = CreatePlayer(slot);
            if (player is PlayerBase basePlayer) basePlayer.Color = palette[colorIndex % palette.Length];
            colorIndex++;
            var playerId = player.Id;
            players.Add(player);

            var configuredSiteName = GetConfiguredSiteName(sectorMetadata, slot.HeadquartersSectorId);
            var desiredSiteName = string.IsNullOrWhiteSpace(slot.HeadquartersSiteName)
                ? configuredSiteName
                : slot.HeadquartersSiteName;

            var site = siteAllocator.Allocate(desiredSiteName);

            if (!sectors.TryAdd(slot.HeadquartersSectorId, new Sector(slot.HeadquartersSectorId, site, playerId)))
                throw new InvalidOperationException(
                    $"Sector '{slot.HeadquartersSectorId}' is assigned to multiple players.");

            var gangData = ResolveRequired(gangLookup, slot.StartingGangName, "Starting gang");
            gangs.Add(new Gang(Guid.NewGuid(), gangData, playerId, slot.HeadquartersSectorId));
        }

        // Include any additional neutral sectors defined on the scenario.
        foreach (var sectorId in config.MapSectorIds)
        {
            if (string.IsNullOrWhiteSpace(sectorId) || sectors.ContainsKey(sectorId)) continue;

            var configuredSiteName = GetConfiguredSiteName(sectorMetadata, sectorId);
            var site = siteAllocator.Allocate(configuredSiteName);
            sectors[sectorId] = new Sector(sectorId, site);
        }

        foreach (var entry in sectorMetadata)
        {
            var sectorId = entry.Key;
            if (sectors.ContainsKey(sectorId)) continue;

            var site = siteAllocator.Allocate(entry.Value);
            sectors[sectorId] = new Sector(sectorId, site);
        }

        var game = new Game(players, sectors.Values, gangs);
        var startingIndex = players.FindIndex(p => p is Player);
        if (startingIndex < 0) startingIndex = 0;

        return new GameState(game, config, players, startingIndex, seed);
    }

    private static void ValidateConfig(ScenarioConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Name))
            throw new ArgumentException("Scenario name is required.", nameof(config));

        if (config.Players is null || config.Players.Count == 0)
            throw new ArgumentException("At least one player configuration must be provided.", nameof(config));

        var sectorIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var player in config.Players)
        {
            if (string.IsNullOrWhiteSpace(player.Name))
                throw new ArgumentException("Player name is required.", nameof(config));

            if (string.IsNullOrWhiteSpace(player.HeadquartersSectorId))
                throw new ArgumentException("Headquarters sector id is required for all players.", nameof(config));

            if (!sectorIds.Add(player.HeadquartersSectorId))
                throw new ArgumentException($"Duplicate headquarters sector '{player.HeadquartersSectorId}' detected.",
                    nameof(config));

            if (string.IsNullOrWhiteSpace(player.StartingGangName))
                throw new ArgumentException("Starting gang name is required for all players.", nameof(config));

            if (player.StartingCash < 0)
                throw new ArgumentOutOfRangeException(nameof(config), player.StartingCash,
                    "Starting cash cannot be negative.");
        }
    }

    private static IPlayer CreatePlayer(ScenarioPlayerConfig slot)
    {
        var id = Guid.NewGuid();
        return slot.Kind switch
        {
            PlayerKind.Human => new Player(id, slot.Name, slot.StartingCash),
            PlayerKind.AiEasy => new AiPlayer(id, slot.Name, AiDifficulty.Easy, slot.StartingCash),
            PlayerKind.AiMedium => new AiPlayer(id, slot.Name, AiDifficulty.Medium, slot.StartingCash),
            PlayerKind.AiHard => new AiPlayer(id, slot.Name, AiDifficulty.Hard, slot.StartingCash),
            PlayerKind.Network => new NetworkPlayer(id, slot.Name, slot.StartingCash),
            _ => throw new ArgumentOutOfRangeException(nameof(slot.Kind), slot.Kind, "Unsupported player kind.")
        };
    }

    private async Task<IReadOnlyDictionary<string, GangData>> BuildGangLookupAsync(CancellationToken cancellationToken)
    {
        var gangs = await _dataService.GetGangsAsync(cancellationToken).ConfigureAwait(false);
        return gangs.ToDictionary(g => g.Name, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<IReadOnlyDictionary<string, SiteData>> BuildSiteLookupAsync(CancellationToken cancellationToken)
    {
        var sites = await _dataService.GetSitesAsync(cancellationToken).ConfigureAwait(false);
        return sites.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<IReadOnlyDictionary<string, string?>> BuildSectorMetadataAsync(
        CancellationToken cancellationToken)
    {
        var configuration = await _dataService.GetSectorConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (configuration is null)
            throw new InvalidOperationException("Sector configuration data could not be loaded.");

        if (configuration.Sectors is null || configuration.Sectors.Count == 0)
            throw new InvalidOperationException("Sector configuration must define at least one sector entry.");

        var sectors = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var sectorData in configuration.Sectors)
        {
            if (string.IsNullOrWhiteSpace(sectorData.Id)) continue;

            sectors[sectorData.Id] = string.IsNullOrWhiteSpace(sectorData.SiteName)
                ? null
                : sectorData.SiteName;
        }

        return sectors;
    }

    private static GangData ResolveRequired(
        IReadOnlyDictionary<string, GangData> lookup,
        string name,
        string context)
    {
        if (!lookup.TryGetValue(name, out var data))
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0} '{1}' was not found.",
                context, name));

        return data;
    }

    private static string? GetConfiguredSiteName(
        IReadOnlyDictionary<string, string?> lookup,
        string sectorId)
    {
        if (!lookup.TryGetValue(sectorId, out var siteName))
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                "Sector '{0}' is not defined in the sector configuration.", sectorId));

        return siteName;
    }

    private sealed class SiteAllocator
    {
        private readonly List<SiteData> _allSites;
        private readonly List<SiteData> _available;
        private readonly IReadOnlyDictionary<string, SiteData> _lookup;
        private readonly IRngService _rngService;

        public SiteAllocator(IReadOnlyDictionary<string, SiteData> lookup, IRngService rngService)
        {
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));
            _rngService = rngService ?? throw new ArgumentNullException(nameof(rngService));

            _allSites = lookup.Values.ToList();
            if (_allSites.Count == 0) throw new InvalidOperationException("Site data is required to seed scenarios.");

            _available = new List<SiteData>(_allSites);
            Shuffle(_available);
        }

        public SiteData Allocate(string? requestedName)
        {
            if (!string.IsNullOrWhiteSpace(requestedName)) return AllocateSpecific(requestedName);

            return AllocateRandom();
        }

        private SiteData AllocateSpecific(string siteName)
        {
            if (!_lookup.TryGetValue(siteName, out var site))
                throw new InvalidOperationException($"Configured site '{siteName}' was not found in the site data.");

            RemoveFromAvailable(site.Name);
            return site;
        }

        private SiteData AllocateRandom()
        {
            if (_available.Count == 0) Refill();

            var lastIndex = _available.Count - 1;
            var site = _available[lastIndex];
            _available.RemoveAt(lastIndex);
            return site;
        }

        private void RemoveFromAvailable(string siteName)
        {
            var index = _available.FindIndex(site =>
                string.Equals(site.Name, siteName, StringComparison.OrdinalIgnoreCase));
            if (index >= 0) _available.RemoveAt(index);

            if (_available.Count == 0) Refill();
        }

        private void Refill()
        {
            _available.Clear();
            _available.AddRange(_allSites);
            Shuffle(_available);
        }

        private void Shuffle(List<SiteData> sites)
        {
            for (var i = sites.Count - 1; i > 0; i--)
            {
                var swapIndex = _rngService.NextInt(0, i + 1);
                (sites[i], sites[swapIndex]) = (sites[swapIndex], sites[i]);
            }
        }
    }
}