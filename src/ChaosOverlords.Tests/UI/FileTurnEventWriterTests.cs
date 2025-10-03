using ChaosOverlords.Core.Configuration;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Events;

namespace ChaosOverlords.Tests.UI;

public class FileTurnEventWriterTests
{
    [Fact]
    public void CreatesDirectory_WritesHeader_And_Appends()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "co_logs_" + Guid.NewGuid().ToString("N"));
        try
        {
            var options = new LoggingOptions
                { Enabled = true, LogDirectory = tempDir, FileNamePrefix = "turn_test", MaxRetainedFiles = 2 };
            var log = new TurnEventLog();
            var pathProvider = new TestPathProvider(options);
            // Create, write, then dispose before reading to avoid FileShare conflicts on Windows.
            using (var writer = new FileTurnEventWriter(log, options, pathProvider))
            {
                writer.Write(1, TurnPhase.Execution, TurnEventType.Information, "hello");
            }

            var files = Directory.GetFiles(tempDir, "turn_test-*.log");
            Assert.NotEmpty(files);

            // Read with shared access and a tiny retry in case the OS still finalizes the handle.
            var content = ReadAllTextShared(files[0]);
            Assert.Contains("# Session started", content);
            Assert.Contains("hello", content);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    private static string ReadAllTextShared(string path)
    {
        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var sr = new StreamReader(fs);
                return sr.ReadToEnd();
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                // Wait briefly and retry to avoid flakiness on Windows file locking.
                Thread.Sleep(25);
            }

        // Final attempt without catching to surface the error.
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var sr = new StreamReader(fs))
        {
            return sr.ReadToEnd();
        }
    }

    private sealed class TestPathProvider(LoggingOptions options) : ILogPathProvider
    {
        public string GetLogDirectory()
        {
            return options.LogDirectory;
        }
    }
}