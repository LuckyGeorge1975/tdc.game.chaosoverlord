using System;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Economy;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Provides a deterministic projection of upcoming financial changes for the active player.
/// </summary>
public interface IFinancePreviewService
{
    FinanceProjection BuildProjection(GameState gameState, Guid playerId);
}
