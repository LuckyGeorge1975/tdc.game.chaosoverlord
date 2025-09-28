// NOTE: Currently not used; retained as a potential blueprint until UI bindings require it.
namespace ChaosOverlords.Core.Domain.Reference;

/// <summary>
/// Blueprint for a city site the player can capture or develop during the campaign.
/// </summary>
public sealed record SiteModel(
    string Id,
    string Name,
    SiteAttributes Attributes,
    StatModifiers Modifiers,
    string Image,
    string Thumbnail)
{
    public override string ToString() => Name;
}

/// <summary>
/// Economic leverage and district-specific effects unlocked when the site is under player control.
/// </summary>
public sealed record SiteAttributes(
    int Resistance,
    int Support,
    int Tolerance,
    int Cash,
    int EquipmentDiscountPercent,
    int EnablesResearchThroughTechLevel,
    int Security);
