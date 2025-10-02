using ChaosOverlords.Core.Domain.Players;
using ChaosOverlords.Core.Domain.Scenario;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Provides a lightweight default scenario for single-player happy-path testing.
/// </summary>
public sealed class DefaultScenarioProvider : IDefaultScenarioProvider
{
    public ScenarioConfig CreateScenario() => new()
    {
        Type = ScenarioType.KillEmAll,
        Name = "Happy Path Seed",
        Seed = 12345,
        Players = new List<ScenarioPlayerConfig>
        {
            new()
            {
                Name = "Player One",
                Kind = PlayerKind.Human,
                StartingCash = 250,
                HeadquartersSectorId = "D4",
                HeadquartersSiteName = "Arena",
                // Use a stronger starter so Control at HQ isn't auto-fail in the demo
                StartingGangName = "Angels Of Arcadia"
            },
            new()
            {
                Name = "CPU",
                Kind = PlayerKind.AiEasy,
                StartingCash = 150,
                HeadquartersSectorId = "E5",
                HeadquartersSiteName = "Arboretum",
                StartingGangName = "Abominators"
            }
        },
        MapSectorIds = new List<string>
        {
            "C3", "C4", "C5",
            "D3", "D5",
            "E3", "E4", "E6",
            "F4", "F5"
        }
    };
}
