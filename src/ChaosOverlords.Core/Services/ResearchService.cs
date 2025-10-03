using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Core.Services;

public sealed class ResearchService : IResearchService
{
    private readonly IDataService? _dataService;

    public ResearchService()
    {
        // Parameterless for tests that don't require data validation.
    }

    public ResearchService(IDataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
    }

    public ResearchPreview BuildPreview(GameState state, Guid playerId)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));

        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));

        var game = state.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetPlayer(playerId, out var player) || player is null) return new ResearchPreview(null, 0, 0);

        // Estimated progress: sum of Research stats of all player's gangs currently on board.
        var researchPower = game.Gangs.Values
            .Where(g => g.OwnerId == playerId)
            .Select(g => Math.Max(0, g.TotalStats.Research))
            .DefaultIfEmpty(0)
            .Sum();

        return new ResearchPreview(
            state.Research.TryGet(playerId, out var pr) && pr is not null ? pr.ActiveProjectId : null, researchPower,
            0);
    }

    public ResearchActionResult Execute(GameState state, Guid playerId, string projectId, int turnNumber)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));

        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));

        if (string.IsNullOrWhiteSpace(projectId))
            return new ResearchActionResult(ResearchActionStatus.Failed, "Project id must be provided.", null, 0, 0);

        var game = state.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetPlayer(playerId, out _))
            return new ResearchActionResult(ResearchActionStatus.Failed, "Player not found.", null, 0, 0);

        var researchPower = game.Gangs.Values
            .Where(g => g.OwnerId == playerId)
            .Select(g => Math.Max(0, g.TotalStats.Research))
            .DefaultIfEmpty(0)
            .Sum();

        // If a data service is available, validate the project against items and use their properties.
        ItemData? item = null;
        if (_dataService is not null)
        {
            var items = _dataService.GetItemsAsync().GetAwaiter().GetResult();
            item = items.FirstOrDefault(i => string.Equals(i.Name, projectId, StringComparison.Ordinal));
            if (item is null)
                return new ResearchActionResult(ResearchActionStatus.Failed, $"Unknown research project: {projectId}",
                    null, 0, 0);
        }

        var playerResearch = state.Research.GetOrCreate(playerId);
        playerResearch.SelectProject(projectId);
        playerResearch.AddProgress(researchPower);

        var total = playerResearch.Progress;
        var required = item?.ResearchCost ?? 0;
        var completed = required > 0 && total >= required;
        if (completed) playerResearch.UnlockedItems.Add(projectId);

        var statusMsg = researchPower > 0
            ? $"Research progressed: +{researchPower} on {projectId}."
            : $"No research progress this turn for {projectId}.";

        var message = completed
            ? string.Concat(statusMsg, " Completed!")
            : required > 0
                ? string.Concat(statusMsg, $" ({total}/{required})")
                : statusMsg;

        return new ResearchActionResult(ResearchActionStatus.Success, message, projectId, researchPower, 0);
    }
}