using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;
using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Default implementation that wires reference data into runtime game state for freshly created campaigns.
/// </summary>
public sealed class ScenarioService(IDataService dataService) : IScenarioService
{
    private readonly IDataService _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));

    public async Task<GameState> CreateNewGameAsync(ScenarioConfig config, CancellationToken cancellationToken = default)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        ValidateConfig(config);

        var gangLookup = await BuildGangLookupAsync(cancellationToken).ConfigureAwait(false);
        var siteLookup = await BuildSiteLookupAsync(cancellationToken).ConfigureAwait(false);

        var players = new List<IPlayer>();
        var sectors = new Dictionary<string, Sector>(StringComparer.OrdinalIgnoreCase);
        var gangs = new List<Gang>();

        foreach (var slot in config.Players)
        {
            var player = CreatePlayer(slot);
            var playerId = player.Id;
            players.Add(player);

            var site = ResolveOptional(siteLookup, slot.HeadquartersSiteName);
            if (!sectors.TryAdd(slot.HeadquartersSectorId, new Sector(slot.HeadquartersSectorId, site, playerId)))
            {
                throw new InvalidOperationException($"Sector '{slot.HeadquartersSectorId}' is assigned to multiple players.");
            }

            var gangData = ResolveRequired(gangLookup, slot.StartingGangName, "Starting gang");
            gangs.Add(new Gang(Guid.NewGuid(), gangData, playerId, slot.HeadquartersSectorId));
        }

        // Include any additional neutral sectors defined on the scenario.
        foreach (var sectorId in config.MapSectorIds)
        {
            if (string.IsNullOrWhiteSpace(sectorId))
            {
                continue;
            }

            sectors.TryAdd(sectorId, new Sector(sectorId));
        }

        var game = new Game(players, sectors.Values, gangs);
        var startingIndex = players.FindIndex(p => p is Player);
        if (startingIndex < 0)
        {
            startingIndex = 0;
        }

        return new GameState(game, config, players, startingIndex);
    }

    private static void ValidateConfig(ScenarioConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Name))
        {
            throw new ArgumentException("Scenario name is required.", nameof(config));
        }

        if (config.Players is null || config.Players.Count == 0)
        {
            throw new ArgumentException("At least one player configuration must be provided.", nameof(config));
        }

        var sectorIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var player in config.Players)
        {
            if (string.IsNullOrWhiteSpace(player.Name))
            {
                throw new ArgumentException("Player name is required.", nameof(config));
            }

            if (string.IsNullOrWhiteSpace(player.HeadquartersSectorId))
            {
                throw new ArgumentException("Headquarters sector id is required for all players.", nameof(config));
            }

            if (!sectorIds.Add(player.HeadquartersSectorId))
            {
                throw new ArgumentException($"Duplicate headquarters sector '{player.HeadquartersSectorId}' detected.", nameof(config));
            }

            if (string.IsNullOrWhiteSpace(player.StartingGangName))
            {
                throw new ArgumentException("Starting gang name is required for all players.", nameof(config));
            }

            if (player.StartingCash < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(config), player.StartingCash, "Starting cash cannot be negative.");
            }
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
            _ => throw new ArgumentOutOfRangeException(nameof(slot.Kind), slot.Kind, "Unsupported player kind."),
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

    private static GangData ResolveRequired(
        IReadOnlyDictionary<string, GangData> lookup,
        string name,
        string context)
    {
        if (!lookup.TryGetValue(name, out var data))
        {
            throw new InvalidOperationException($"{context} '{name}' was not found.");
        }

        return data;
    }

    private static SiteData? ResolveOptional(
        IReadOnlyDictionary<string, SiteData> lookup,
        string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (!lookup.TryGetValue(name, out var data))
        {
            throw new InvalidOperationException($"Headquarters site '{name}' was not found.");
        }

        return data;
    }
}
