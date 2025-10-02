using System.Collections.ObjectModel;
using ChaosOverlords.App.ViewModels;
using ChaosOverlords.App.ViewModels.Sections;
using Xunit;

namespace ChaosOverlords.Tests.UI;

public class TurnEventsAutoScrollTests
{
    [Fact]
    public void AutoScroll_DefaultsToTrue()
    {
        var events = new ObservableCollection<TurnViewModel.TurnEventViewModel>();
        var vm = new TurnEventsSectionViewModel(events);
        Assert.True(vm.AutoScrollToLatest);
    }
}
