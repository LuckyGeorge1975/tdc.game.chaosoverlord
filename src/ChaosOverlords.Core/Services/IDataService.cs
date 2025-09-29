using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Provides read-only access to the static game data used by the game.
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Retrieves all gang data records.
    /// </summary>
    Task<IReadOnlyList<GangData>> GetGangsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all gang data records synchronously when asynchronous access is not feasible.
    /// </summary>
    IReadOnlyList<GangData> GetGangs();

    /// <summary>
    /// Retrieves all item data records.
    /// </summary>
    Task<IReadOnlyList<ItemData>> GetItemsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all site data records.
    /// </summary>
    Task<IReadOnlyList<SiteData>> GetSitesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the mapping of item type ids to descriptive metadata.
    /// </summary>
    Task<IReadOnlyDictionary<int, ItemTypeData>> GetItemTypesAsync(CancellationToken cancellationToken = default);
}
