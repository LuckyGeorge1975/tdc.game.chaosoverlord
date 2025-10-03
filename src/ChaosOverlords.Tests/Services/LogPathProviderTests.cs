using ChaosOverlords.Core.Configuration;
using ChaosOverlords.Core.Domain.Game.Events;

namespace ChaosOverlords.Tests.Services;

public class LogPathProviderTests
{
    [Fact]
    public void ReturnsAbsolutePath_WhenConfiguredRelative()
    {
        var options = new LoggingOptions { LogDirectory = "relative_path_tests" };
        var provider = new LogPathProvider(options);
        var dir = provider.GetLogDirectory();
        Assert.True(Path.IsPathRooted(dir));
    }
}