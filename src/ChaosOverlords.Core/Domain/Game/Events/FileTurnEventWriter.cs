using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ChaosOverlords.Core.Configuration;
using ChaosOverlords.Core.Domain.Game.Actions;
using ChaosOverlords.Core.Domain.Game.Economy;

namespace ChaosOverlords.Core.Domain.Game.Events;

/// <summary>
/// ITurnEventWriter that mirrors events to an in-memory log and a session-scoped file.
/// </summary>
public sealed class FileTurnEventWriter : ITurnEventWriter, IDisposable
{
    private readonly ITurnEventLog _eventLog;
    private readonly LoggingOptions _options;
    private readonly ILogPathProvider _pathProvider;
    private readonly object _sync = new();
    private readonly string _logFilePath;
    private StreamWriter? _writer;
    private bool _disposed;

    public FileTurnEventWriter(ITurnEventLog eventLog, LoggingOptions options, ILogPathProvider pathProvider)
    {
        _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));

    _logFilePath = CreateLogFilePath(_options, _pathProvider);
        TryTrimOldLogs();
        if (_options.Enabled)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath)!);
            // Use ReadWrite sharing so tests/tools can read the file while the writer holds it open.
            _writer = new StreamWriter(new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                AutoFlush = true
            };
            WriteSessionHeader();
        }
    }

    public void Write(int turnNumber, TurnPhase phase, TurnEventType type, string description, CommandPhase? commandPhase = null)
    {
        if (turnNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(turnNumber), turnNumber, "Turn number must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description must be provided.", nameof(description));
        }

        var entry = new TurnEvent(turnNumber, phase, commandPhase, type, description, DateTimeOffset.UtcNow);
        _eventLog.Append(entry);

        if (!_options.Enabled)
        {
            return;
        }

        lock (_sync)
        {
            _writer?.WriteLine(FormatEventLine(entry));
        }
    }

    public void WriteEconomy(int turnNumber, TurnPhase phase, PlayerEconomySnapshot snapshot)
    {
        if (snapshot is null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        var description = TurnEventWriterReflection.FormatEconomyDescription(snapshot);
        Write(turnNumber, phase, TurnEventType.Economy, description);
    }

    public void WriteAction(ActionResult result)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        var context = result.Context;
        var description = TurnEventWriterReflection.FormatActionDescription(result);
        Write(context.TurnNumber, context.Phase, TurnEventType.Action, description, context.CommandPhase);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_sync)
        {
            _writer?.Dispose();
            _writer = null;
            _disposed = true;
        }
    }

    private void WriteSessionHeader()
    {
        var header = $"# Session started {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}";
        lock (_sync)
        {
            _writer?.WriteLine(header);
        }
    }

    private static string CreateLogFilePath(LoggingOptions options, ILogPathProvider pathProvider)
    {
        var directory = pathProvider.GetLogDirectory();
        var fileName = string.Format(CultureInfo.InvariantCulture, "{0}-{1:yyyyMMdd_HHmmss}.log", options.FileNamePrefix ?? "turn", DateTimeOffset.Now);
        return Path.Combine(directory, fileName);
    }

    private void TryTrimOldLogs()
    {
        if (_options.MaxRetainedFiles <= 0)
        {
            return;
        }

        try
        {
            var directory = _pathProvider.GetLogDirectory();
            if (!Directory.Exists(directory))
            {
                return;
            }

            var prefix = (_options.FileNamePrefix ?? "turn") + "-";
            var files = new DirectoryInfo(directory)
                .EnumerateFiles("*.log", SearchOption.TopDirectoryOnly)
                .Where(f => f.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => f.CreationTimeUtc)
                .ThenByDescending(f => f.LastWriteTimeUtc)
                .ToList();

            if (files.Count <= _options.MaxRetainedFiles)
            {
                return;
            }

            foreach (var file in files.Skip(_options.MaxRetainedFiles))
            {
                try { file.Delete(); } catch { /* ignore */ }
            }
        }
        catch
        {
            // non-fatal
        }
    }

    private static string FormatEventLine(TurnEvent entry)
    {
        var ts = entry.Timestamp.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var phase = entry.CommandPhase.HasValue ? $"{entry.Phase}/{entry.CommandPhase}" : entry.Phase.ToString();
        return string.Format(CultureInfo.InvariantCulture, "[{0}] T{1} {2} {3}: {4}", ts, entry.TurnNumber, phase, entry.Type, entry.Description);
    }
}

/// <summary>
/// Internal helpers exposing the existing formatting logic to avoid duplication.
/// </summary>
internal static class TurnEventWriterReflection
{
    public static string FormatEconomyDescription(PlayerEconomySnapshot snapshot)
        => typeof(TurnEventWriter)
            .GetMethod("FormatEconomyDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { snapshot })!.ToString()!;

    public static string FormatActionDescription(ActionResult result)
        => typeof(TurnEventWriter)
            .GetMethod("FormatActionDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { result })!.ToString()!;
}