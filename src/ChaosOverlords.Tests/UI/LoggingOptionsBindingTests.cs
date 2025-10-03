using ChaosOverlords.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChaosOverlords.Tests.UI;

public class LoggingOptionsBindingTests
{
    [Fact]
    public void Binds_From_Appsettings_Json()
    {
        // Arrange: create a temp appsettings.json
        var tempDir = Path.Combine(Path.GetTempPath(), "co_logs_binding_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var jsonPath = Path.Combine(tempDir, "appsettings.json");
        File.WriteAllText(jsonPath, @"{
    ""Logging"": {
        ""TurnEvents"": {
            ""Enabled"": true,
            ""LogDirectory"": ""log_out"",
            ""FileNamePrefix"": ""turn_bind"",
            ""MaxRetainedFiles"": 7
        }
    }
}");

        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(tempDir)
                .AddJsonFile("appsettings.json", false)
                .Build();

            var services = new ServiceCollection();
            services.Configure<LoggingOptions>(config.GetSection("Logging:TurnEvents"));
            var provider = services.BuildServiceProvider();

            var bound = provider.GetRequiredService<IOptions<LoggingOptions>>().Value;

            Assert.True(bound.Enabled);
            Assert.Equal("log_out", bound.LogDirectory);
            Assert.Equal("turn_bind", bound.FileNamePrefix);
            Assert.Equal(7, bound.MaxRetainedFiles);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}