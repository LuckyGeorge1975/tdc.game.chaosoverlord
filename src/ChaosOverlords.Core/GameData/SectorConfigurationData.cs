using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ChaosOverlords.Core.GameData;

/// <summary>
/// Container describing the static sector metadata used to seed campaigns.
/// </summary>
public sealed class SectorConfigurationData
{
    public IReadOnlyList<SectorDefinitionData> Sectors { get; init; } = Array.Empty<SectorDefinitionData>();

    public SectorConfigurationData Normalize()
    {
        return new SectorConfigurationData
        {
            Sectors = new ReadOnlyCollection<SectorDefinitionData>((Sectors ?? Array.Empty<SectorDefinitionData>()).ToList())
        };
    }
}

/// <summary>
/// Associates a sector id with an optional preconfigured site assignment.
/// </summary>
public sealed class SectorDefinitionData
{
    public string Id { get; init; } = string.Empty;

    public string? SiteName { get; init; }
}
