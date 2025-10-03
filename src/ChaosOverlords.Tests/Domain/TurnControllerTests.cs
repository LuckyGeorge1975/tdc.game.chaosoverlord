using ChaosOverlords.Core.Domain.Game;

namespace ChaosOverlords.Tests.Domain;

public class TurnControllerTests
{
    [Fact]
    public void EndTurn_enforces_elimination_phase()
    {
        var controller = new TurnController();

        Assert.False(controller.CanEndTurn);

        controller.StartTurn();

        // Progress through main phases, including command sub-phases.
        while (controller.CurrentPhase != TurnPhase.Elimination)
        {
            Assert.True(controller.CanAdvancePhase);
            controller.AdvancePhase();
        }

        Assert.True(controller.CanEndTurn);
        controller.EndTurn();

        Assert.Equal(TurnPhase.Upkeep, controller.CurrentPhase);
        Assert.False(controller.IsTurnActive);
        Assert.Equal(2, controller.TurnNumber);
    }
}