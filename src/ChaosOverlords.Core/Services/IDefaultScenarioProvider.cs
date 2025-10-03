using ChaosOverlords.Core.Domain.Scenario;

namespace ChaosOverlords.Core.Services;

/// <summary>
///     Supplies the baseline scenario configuration used when bootstrapping a new campaign session.
/// </summary>
public interface IDefaultScenarioProvider
{
    ScenarioConfig CreateScenario();
}