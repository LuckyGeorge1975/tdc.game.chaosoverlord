// NOTE: Currently not used; kept for future view-layer modeling experiments.
namespace ChaosOverlords.Core.Domain.Reference;

/// <summary>
/// Blueprint for a recruitable gang, capturing the static stats the player sees on the hiring screen.
/// </summary>
public sealed record GangModel(
    string Id,
    string Name,
    GangStats Stats,
    string Image,
    string Thumbnail)
{
    public override string ToString() => Name;
}

/// <summary>
/// Combat, economy, and utility ratings that drive how the gang performs once deployed in a sector.
/// </summary>
public sealed record GangStats(
    int HiringCost,
    int UpkeepCost,
    int Combat,
    int Defense,
    int TechLevel,
    int Stealth,
    int Detect,
    int Chaos,
    int Control,
    int Heal,
    int Influence,
    int Research,
    int Strength,
    int BladeMelee,
    int Ranged,
    int Fighting,
    int MartialArts);
