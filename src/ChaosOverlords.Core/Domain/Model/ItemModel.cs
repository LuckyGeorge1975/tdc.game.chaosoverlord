// NOTE: Currently not used; retained to capture potential item-view projections.
namespace ChaosOverlords.Core.Domain.Reference;

/// <summary>
/// Blueprint for equipment the player can research, craft, and assign to gangs.
/// </summary>
public sealed record ItemModel(
    string Id,
    string Name,
    ItemType Type,
    ItemCosts Costs,
    StatModifiers Modifiers,
    string Image,
    string Thumbnail)
{
    public override string ToString() => Name;
}

/// <summary>
/// Research and fabrication effort required before the item can enter the armory.
/// </summary>
public sealed record ItemCosts(
    int ResearchCost,
    int FabricationCost,
    int TechLevel);

/// <summary>
/// Categorizes equipment for UI grouping and gameplay restrictions.
/// </summary>
public enum ItemType
{
    Melee = 1,
    Ranged = 2,
    Armor = 3,
    Misc = 4
}

/// <summary>
/// Stat bonuses the item grants to the equipped gang or site.
/// </summary>
public sealed record StatModifiers(
    int Combat,
    int Defense,
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
