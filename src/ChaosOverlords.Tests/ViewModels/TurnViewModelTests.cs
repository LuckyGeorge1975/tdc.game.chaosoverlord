using ChaosOverlords.App.ViewModels;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Events;
using Xunit;

namespace ChaosOverlords.Tests.ViewModels;

public class TurnViewModelTests
{
    [Fact]
    public void EndTurnCommand_is_enabled_in_elimination_phase()
    {
    var controller = new TurnController();
    using var viewModel = new TurnViewModel(controller, new TurnEventLog());

        Assert.False(viewModel.EndTurnCommand.CanExecute(null));

        viewModel.StartTurnCommand.Execute(null);

        // The maximum number of phase advances is set to 32 as a safety guard to prevent infinite loops.
        // This should be greater than or equal to the number of phases in the turn state machine.
        var maxPhaseAdvances = 32;
        var guard = maxPhaseAdvances;
        while (viewModel.CurrentPhase != TurnPhase.Elimination && guard-- > 0)
        {
            if (!viewModel.AdvancePhaseCommand.CanExecute(null))
            {
                break;
            }

            viewModel.AdvancePhaseCommand.Execute(null);
        }

        Assert.True(guard > 0, $"Failed to reach elimination phase within {maxPhaseAdvances} advances.");
        Assert.Equal(TurnPhase.Elimination, viewModel.CurrentPhase);
        Assert.True(viewModel.EndTurnCommand.CanExecute(null));
    }
}
