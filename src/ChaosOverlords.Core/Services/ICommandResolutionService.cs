using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Commands;

namespace ChaosOverlords.Core.Services;

/// <summary>
///     Resolves queued commands during the execution phase.
/// </summary>
public interface ICommandResolutionService
{
    CommandExecutionReport Execute(GameState gameState, Guid playerId, int turnNumber);
}