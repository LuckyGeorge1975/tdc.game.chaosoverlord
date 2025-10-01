using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChaosOverlords.Core.GameData;
using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Data;

/// <summary>
/// Loads static game data from JSON files embedded within the data assembly.
/// </summary>
public sealed class EmbeddedJsonDataService : IDataService
{
    private const string GangsResource = "ChaosOverlords.Data.Assets.gangs.json";
    private const string ItemsResource = "ChaosOverlords.Data.Assets.items.json";
    private const string SitesResource = "ChaosOverlords.Data.Assets.sites.json";
    private const string ItemTypesResource = "ChaosOverlords.Data.Assets.item_types.json";
    private const string SectorsResource = "ChaosOverlords.Data.Assets.sectors.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly IReadOnlyList<GangData> _gangs;
    private readonly IReadOnlyList<ItemData> _items;
    private readonly IReadOnlyList<SiteData> _sites;
    private readonly IReadOnlyDictionary<int, ItemTypeData> _itemTypes;
    private readonly SectorConfigurationData _sectorConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedJsonDataService"/> class.
    /// </summary>
    public EmbeddedJsonDataService()
    {
        var assembly = Assembly.GetExecutingAssembly();
        _gangs = LoadList<GangData>(assembly, GangsResource);
        _items = LoadList<ItemData>(assembly, ItemsResource);
        _sites = LoadList<SiteData>(assembly, SitesResource);
    _sectorConfiguration = Load<SectorConfigurationData>(assembly, SectorsResource).Normalize();

        var itemTypeList = LoadList<ItemTypeData>(assembly, ItemTypesResource);
        var itemTypeDictionary = itemTypeList.ToDictionary(static t => t.Id);
        _itemTypes = new ReadOnlyDictionary<int, ItemTypeData>(itemTypeDictionary);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<GangData>> GetGangsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_gangs);
    }

    /// <inheritdoc />
    public IReadOnlyList<GangData> GetGangs() => _gangs;

    /// <inheritdoc />
    public Task<IReadOnlyList<ItemData>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_items);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<SiteData>> GetSitesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_sites);
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<int, ItemTypeData>> GetItemTypesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_itemTypes);
    }

    /// <inheritdoc />
    public Task<SectorConfigurationData> GetSectorConfigurationAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_sectorConfiguration);
    }

    private static IReadOnlyList<T> LoadList<T>(Assembly assembly, string resourceName)
    {
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Embedded resource '{resourceName}' could not be found.");
        }

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var data = JsonSerializer.Deserialize<List<T>>(json, SerializerOptions);
        if (data is null)
        {
            throw new InvalidOperationException($"Embedded resource '{resourceName}' did not contain valid JSON.");
        }

        return new ReadOnlyCollection<T>(data);
    }

    private static T Load<T>(Assembly assembly, string resourceName)
    {
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Embedded resource '{resourceName}' could not be found.");
        }

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var data = JsonSerializer.Deserialize<T>(json, SerializerOptions);
        if (data is null)
        {
            throw new InvalidOperationException($"Embedded resource '{resourceName}' did not contain valid JSON.");
        }

        return data;
    }
}
