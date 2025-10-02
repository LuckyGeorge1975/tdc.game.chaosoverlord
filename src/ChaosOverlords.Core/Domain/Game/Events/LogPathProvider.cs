using System;
using System.IO;
using ChaosOverlords.Core.Configuration;

namespace ChaosOverlords.Core.Domain.Game.Events;

public sealed class LogPathProvider : ILogPathProvider
{
    private readonly LoggingOptions _options;

    public LogPathProvider(LoggingOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public string GetLogDirectory()
    {
        var configured = _options.LogDirectory;
        if (string.IsNullOrWhiteSpace(configured))
        {
            return Path.Combine(AppContext.BaseDirectory, "log");
        }

        if (Path.IsPathRooted(configured))
        {
            return configured;
        }

        return Path.GetFullPath(configured, Directory.GetCurrentDirectory());
    }
}
