using System.Collections.Generic;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Economy;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Default economy processor that applies upkeep, sector tax, and site modifiers.
/// </summary>
public sealed class EconomyService : IEconomyService
{
    public EconomyReport ApplyUpkeep(GameState gameState, int turnNumber)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (turnNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(turnNumber), turnNumber, "Turn number must be greater than zero.");
        }

        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        var players = game.Players.Values;
        var gangs = game.Gangs.Values;
        var sectors = game.Sectors.Values;

    var snapshots = new List<PlayerEconomySnapshot>();

        foreach (var player in players)
        {
            var startingCash = player.Cash;
            var upkeep = gangs.Where(g => g.OwnerId == player.Id).Sum(g => g.Data.UpkeepCost);

            var controlledSectors = sectors.Where(s => s.ControllingPlayerId == player.Id).ToList();
            var sectorIncome = controlledSectors.Sum(s => s.Site?.Cash ?? 0);
            var siteIncome = 0;

            ApplyCashAdjustments(player, upkeep, sectorIncome, siteIncome);

            var endingCash = player.Cash;
            var netChange = endingCash - startingCash;

            snapshots.Add(new PlayerEconomySnapshot(
                player.Id,
                player.Name,
                startingCash,
                upkeep,
                sectorIncome,
                siteIncome,
                netChange,
                endingCash));
        }

        return new EconomyReport(turnNumber, snapshots);
    }

    private static void ApplyCashAdjustments(
        Domain.Players.IPlayer player,
        int upkeep,
        int sectorIncome,
        int siteIncome)
    {
        if (player is null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        if (upkeep > 0)
        {
            player.Debit(upkeep);
        }

        var totalIncome = sectorIncome + siteIncome;
        if (totalIncome > 0)
        {
            player.Credit(totalIncome);
        }
        else if (totalIncome < 0)
        {
            player.Debit(-totalIncome);
        }
    }
}
