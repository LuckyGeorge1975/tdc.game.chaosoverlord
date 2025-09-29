using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
/// Represents a single tile on the city map, optionally tied to immutable <see cref="SiteData"/> for bonuses.
/// </summary>
public sealed class Sector
{
    private readonly List<Guid> _gangIds = new();

    public Sector(string id, SiteData? site = null, Guid? controllingPlayerId = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Sector id cannot be null or whitespace.", nameof(id));
        }

        Id = id;
        Site = site;
        ControllingPlayerId = controllingPlayerId;
    }

    public string Id { get; }

    /// <summary>
    /// Immutable site data that defines passive bonuses when the sector is controlled.
    /// </summary>
    public SiteData? Site { get; }

    public Guid? ControllingPlayerId { get; private set; }

    public IReadOnlyCollection<Guid> GangIds => new ReadOnlyCollection<Guid>(_gangIds);

    /// <summary>
    /// Projected chaos revenue assigned during the chaos sub-phase.
    /// </summary>
    public int ProjectedChaos { get; private set; }

    /// <summary>
    /// Updates the current controller (or clears it) based on campaign actions.
    /// </summary>
    public void SetController(Guid? playerId)
    {
        ControllingPlayerId = playerId;
    }

    /// <summary>
    /// Marks a gang as occupying the sector.
    /// </summary>
    public void PlaceGang(Gang gang)
    {
        if (gang is null)
        {
            throw new ArgumentNullException(nameof(gang));
        }

        if (!_gangIds.Contains(gang.Id))
        {
            _gangIds.Add(gang.Id);
        }
    }

    /// <summary>
    /// Removes a gang from the sector, returning whether it was present.
    /// </summary>
    public bool RemoveGang(Guid gangId) => _gangIds.Remove(gangId);

    /// <summary>
    /// Assigns a projected chaos value to the sector.
    /// </summary>
    public void SetChaosProjection(int value)
    {
        ProjectedChaos = Math.Max(0, value);
    }

    /// <summary>
    /// Clears any existing chaos projection.
    /// </summary>
    public void ClearChaosProjection()
    {
        ProjectedChaos = 0;
    }
}
