namespace ChaosOverlords.Core.Domain.Game.Research;

/// <summary>
///     Tracks research progress per player. Minimal Phase 4 skeleton: single active project id and integer progress.
/// </summary>
public sealed class ResearchState
{
    private readonly Dictionary<Guid, PlayerResearch> _players = new();

    public PlayerResearch GetOrCreate(Guid playerId)
    {
        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));

        if (!_players.TryGetValue(playerId, out var state))
        {
            state = new PlayerResearch(playerId);
            _players[playerId] = state;
        }

        return state;
    }

    public bool TryGet(Guid playerId, out PlayerResearch? state)
    {
        return _players.TryGetValue(playerId, out state);
    }
}

public sealed class PlayerResearch
{
    public PlayerResearch(Guid playerId)
    {
        if (playerId == Guid.Empty) throw new ArgumentException("Player id must be provided.", nameof(playerId));

        PlayerId = playerId;
    }

    public Guid PlayerId { get; }

    /// <summary>
    ///     Currently selected project. For Phase 4 we treat this as a free-form id (e.g., item blueprint name).
    /// </summary>
    public string? ActiveProjectId { get; private set; }

    /// <summary>
    ///     Accumulated progress towards completing the active project.
    /// </summary>
    public int Progress { get; private set; }

    /// <summary>
    ///     Items that have been fully researched by this player and are eligible for fabrication/equip.
    /// </summary>
    public ISet<string> UnlockedItems { get; } = new HashSet<string>(StringComparer.Ordinal);

    public void SelectProject(string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new ArgumentException("Project id must be provided.", nameof(projectId));

        if (!string.Equals(ActiveProjectId, projectId, StringComparison.Ordinal))
        {
            ActiveProjectId = projectId;
            Progress = 0;
        }
    }

    public void AddProgress(int amount)
    {
        if (amount <= 0) return;

        checked
        {
            Progress += amount;
        }
    }

    public bool IsUnlocked(string projectId)
    {
        return !string.IsNullOrWhiteSpace(projectId) && UnlockedItems.Contains(projectId);
    }
}