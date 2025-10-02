namespace ChaosOverlords.Core.Configuration;

/// <summary>
/// Configurable options for turn event logging.
/// </summary>
public sealed class LoggingOptions
{
    /// <summary>
    /// Enables writing turn events to a file in addition to the in-memory log.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Directory where log files will be written. Can be absolute or relative.
    /// Examples: "src/log" (repo style) or "log".
    /// </summary>
    public string LogDirectory { get; set; } = "logs";

    /// <summary>
    /// File name prefix for the created log file (e.g., "turn" → turn-YYYYMMDD_HHmmss.log).
    /// </summary>
    public string FileNamePrefix { get; set; } = "turn";

    /// <summary>
    /// Maximum number of log files to keep in the log directory. Older files beyond this count will be deleted on startup. Set to 0 to disable retention trimming.
    /// </summary>
    public int MaxRetainedFiles { get; set; } = 20;
}