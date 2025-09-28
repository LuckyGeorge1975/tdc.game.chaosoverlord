using ChaosOverlords.App.ViewModels;
using ChaosOverlords.Core.Domain.Game;
using Xunit;

namespace ChaosOverlords.Tests.ViewModels;

public class TurnViewModelTests
{
    [Fact]
    public void EndTurnCommand_is_enabled_in_elimination_phase()
    {
    var controller = new TurnController();
    var viewModel = new TurnViewModel(controller);

        Assert.False(viewModel.EndTurnCommand.CanExecute(null));

        viewModel.StartTurnCommand.Execute(null);

        var guard = 32;
        while (viewModel.CurrentPhase != TurnPhase.Elimination && guard-- > 0)
        {
            if (!viewModel.AdvancePhaseCommand.CanExecute(null))
            {
                break;
            }

            viewModel.AdvancePhaseCommand.Execute(null);
        }

        Assert.True(guard > 0, "Failed to reach elimination phase while advancing phases.");
        Assert.Equal(TurnPhase.Elimination, viewModel.CurrentPhase);
        Assert.True(viewModel.EndTurnCommand.CanExecute(null));
    }
}
