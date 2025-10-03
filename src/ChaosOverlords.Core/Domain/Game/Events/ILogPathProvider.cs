namespace ChaosOverlords.Core.Domain.Game.Events;

/// <summary>
///     Resolves the effective directory to store/read log files.
/// </summary>
public interface ILogPathProvider
{
    string GetLogDirectory();
}