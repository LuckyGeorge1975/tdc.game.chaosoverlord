namespace ChaosOverlords.Core.Domain.Game.Recruitment;

/// <summary>
///     Stores recruitment pools for all players participating in the current campaign.
/// </summary>
public sealed class RecruitmentState
{
    private readonly Dictionary<Guid, RecruitmentPool> _pools = new();

    internal IReadOnlyDictionary<Guid, RecruitmentPool> Pools => _pools;

    internal RecruitmentPool GetOrCreatePool(Guid playerId, Func<RecruitmentPool> factory)
    {
        if (factory is null) throw new ArgumentNullException(nameof(factory));

        if (!_pools.TryGetValue(playerId, out var pool))
        {
            pool = factory();
            _pools[playerId] = pool;
        }

        return pool;
    }

    internal bool TryGetPool(Guid playerId, out RecruitmentPool? pool)
    {
        return _pools.TryGetValue(playerId, out pool);
    }

    internal IEnumerable<RecruitmentOption> GetAllOptions()
    {
        foreach (var pool in _pools.Values)
        foreach (var option in pool.Options)
            yield return option;
    }
}