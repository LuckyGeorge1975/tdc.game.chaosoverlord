using System;
using System.Collections.Generic;
using System.Linq;
using ChaosOverlords.Core.Domain.Game;
using ChaosOverlords.Core.Domain.Game.Recruitment;
using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Default implementation of <see cref="IRecruitmentService"/> responsible for managing hire pools per player.
/// </summary>
public sealed class RecruitmentService : IRecruitmentService
{
    private const int PoolSize = 3;

    private readonly IReadOnlyList<GangData> _gangData;
    private readonly IRngService _rngService;

    public RecruitmentService(IDataService dataService, IRngService rngService)
    {
        if (dataService is null)
        {
            throw new ArgumentNullException(nameof(dataService));
        }

        _rngService = rngService ?? throw new ArgumentNullException(nameof(rngService));
        _gangData = dataService.GetGangs();
        if (_gangData is null || _gangData.Count == 0)
        {
            throw new InvalidOperationException("Gang data must be available to initialise recruitment service.");
        }
    }

    public RecruitmentPoolSnapshot EnsurePool(GameState gameState, Guid playerId, int turnNumber)
    {
        var result = EnsurePoolInternal(gameState, playerId, turnNumber, CollectReservedNames(gameState));
        return result.Pool;
    }

    public IReadOnlyList<RecruitmentRefreshResult> RefreshPools(GameState gameState, int turnNumber)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (turnNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(turnNumber), turnNumber, "Turn number must be positive.");
        }

        var reservedNames = CollectReservedNames(gameState);
        var results = new List<RecruitmentRefreshResult>();
        foreach (var player in gameState.PlayerOrder)
        {
            var refresh = EnsurePoolInternal(gameState, player.Id, turnNumber, reservedNames);
            results.Add(refresh);
        }

        return results;
    }

    public RecruitmentHireResult Hire(GameState gameState, Guid playerId, Guid optionId, string sectorId, int turnNumber)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (string.IsNullOrWhiteSpace(sectorId))
        {
            throw new ArgumentException("Sector id must be provided.", nameof(sectorId));
        }

        if (turnNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(turnNumber), turnNumber, "Turn number must be positive.");
        }

        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        if (!game.TryGetPlayer(playerId, out var player) || player is null)
        {
            throw new InvalidOperationException($"Player '{playerId}' was not found in the game state.");
        }

        var resolvedPlayer = player;

        if (!game.TryGetSector(sectorId, out var targetSector))
        {
            return FailureHire(gameState, playerId, turnNumber, RecruitmentActionStatus.InvalidSector, optionId, sectorId, "Target sector not found.");
        }

        var sector = targetSector;

        if (sector.ControllingPlayerId != playerId)
        {
            return FailureHire(gameState, playerId, turnNumber, RecruitmentActionStatus.InvalidSector, optionId, sectorId, "Sector is not controlled by the player.");
        }

        var pool = gameState.Recruitment.GetOrCreatePool(playerId, () => CreatePool(gameState, turnNumber, CollectReservedNames(gameState)));
        if (!pool.TryGetOption(optionId, out var option) || option is not { } selectedOption)
        {
            return FailureHire(gameState, playerId, turnNumber, RecruitmentActionStatus.InvalidOption, optionId, sectorId, "Recruitment option not found.");
        }

        if (!selectedOption.CanHire)
        {
            return FailureHire(gameState, playerId, turnNumber, RecruitmentActionStatus.AlreadyResolved, optionId, sectorId, "Recruitment option is no longer available.");
        }

        var hiringCost = selectedOption.GangData.HiringCost;
        if (resolvedPlayer.Cash < hiringCost)
        {
            return FailureHire(gameState, playerId, turnNumber, RecruitmentActionStatus.InsufficientFunds, optionId, sectorId, "Insufficient funds to hire this gang.");
        }

        resolvedPlayer.Debit(hiringCost);
        var gangId = Guid.NewGuid();
        var gang = new Gang(gangId, selectedOption.GangData, playerId, sectorId);
        game.AddGang(gang);

        selectedOption.MarkHired(turnNumber);

        var snapshot = CreatePoolSnapshot(gameState, playerId, pool);
        var optionSnapshot = snapshot.Options.First(o => o.OptionId == selectedOption.Id);
        return new RecruitmentHireResult(RecruitmentActionStatus.Success, snapshot, optionSnapshot, gangId, sectorId, null);
    }

    public RecruitmentDeclineResult Decline(GameState gameState, Guid playerId, Guid optionId, int turnNumber)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (turnNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(turnNumber), turnNumber, "Turn number must be positive.");
        }

        var pool = gameState.Recruitment.GetOrCreatePool(playerId, () => CreatePool(gameState, turnNumber, CollectReservedNames(gameState)));
        if (!pool.TryGetOption(optionId, out var option) || option is not { } selectedOption)
        {
            return FailureDecline(gameState, playerId, turnNumber, RecruitmentActionStatus.InvalidOption, optionId, "Recruitment option not found.");
        }

        if (!selectedOption.CanHire)
        {
            return FailureDecline(gameState, playerId, turnNumber, RecruitmentActionStatus.AlreadyResolved, optionId, "Recruitment option is no longer available.");
        }

        selectedOption.MarkDeclined(turnNumber);
        var snapshot = CreatePoolSnapshot(gameState, playerId, pool);
        var optionSnapshot = snapshot.Options.First(o => o.OptionId == selectedOption.Id);
        return new RecruitmentDeclineResult(RecruitmentActionStatus.Success, snapshot, optionSnapshot, null);
    }

    private RecruitmentRefreshResult EnsurePoolInternal(GameState gameState, Guid playerId, int turnNumber, HashSet<string> reservedNames)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (turnNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(turnNumber), turnNumber, "Turn number must be positive.");
        }

        var game = gameState.Game ?? throw new InvalidOperationException("Game state is missing runtime data.");
        var player = game.GetPlayer(playerId);
        var created = false;
        var pool = gameState.Recruitment.GetOrCreatePool(playerId, () =>
        {
            created = true;
            return CreatePool(gameState, turnNumber, reservedNames);
        });

        var refreshed = RefreshPool(gameState, pool, turnNumber, reservedNames);
        var snapshot = CreatePoolSnapshot(gameState, playerId, pool, player.Name);
        return new RecruitmentRefreshResult(snapshot, created || refreshed);
    }

    private RecruitmentPool CreatePool(GameState gameState, int turnNumber, HashSet<string> reservedNames)
    {
        return new RecruitmentPool(PoolSize, slotIndex =>
        {
            var gang = SelectCandidate(gameState, reservedNames);
            reservedNames.Add(gang.Name);
            return new RecruitmentOption(slotIndex, gang, turnNumber);
        });
    }

    private bool RefreshPool(GameState gameState, RecruitmentPool pool, int turnNumber, HashSet<string> reservedNames)
    {
        var changed = false;
        foreach (var option in pool.Options)
        {
            reservedNames.Remove(option.GangData.Name);

            if (option.NeedsRefresh(turnNumber))
            {
                var gang = SelectCandidate(gameState, reservedNames);
                option.Replace(gang, turnNumber);
                reservedNames.Add(gang.Name);
                changed = true;
            }
            else
            {
                reservedNames.Add(option.GangData.Name);
            }
        }

        return changed;
    }

    private GangData SelectCandidate(GameState gameState, HashSet<string> reservedNames)
    {
        var available = _gangData.Where(g => !reservedNames.Contains(g.Name)).ToList();
        // If no available gangs, throw to prevent duplicate names in recruitment pools.
        if (available.Count == 0)
        {
            throw new InvalidOperationException("No unique gang data available for recruitment. All gang names are reserved.");
        }

        var index = _rngService.NextInt(0, available.Count);
        var candidate = available[index];
        return candidate;
    }

    private HashSet<string> CollectReservedNames(GameState gameState)
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var reserved = new HashSet<string>(comparer);

        foreach (var gang in gameState.Game?.Gangs.Values ?? Enumerable.Empty<Gang>())
        {
            reserved.Add(gang.Data.Name);
        }

        foreach (var option in gameState.Recruitment.GetAllOptions())
        {
            reserved.Add(option.GangData.Name);
        }

        return reserved;
    }

    private RecruitmentPoolSnapshot CreatePoolSnapshot(GameState gameState, Guid playerId, RecruitmentPool pool, string? playerName = null)
    {
        var name = playerName ?? gameState.Game?.GetPlayer(playerId).Name ?? string.Empty;
        var options = pool.Options
            .OrderBy(o => o.SlotIndex)
            .Select(o => new RecruitmentOptionSnapshot(
                o.Id,
                o.SlotIndex,
                o.GangData.Name,
                o.GangData.HiringCost,
                o.GangData.UpkeepCost,
                o.State))
            .ToList();

        return new RecruitmentPoolSnapshot(playerId, name, options);
    }

    private RecruitmentHireResult FailureHire(GameState gameState, Guid playerId, int turnNumber, RecruitmentActionStatus status, Guid optionId, string sectorId, string message)
    {
        var snapshot = EnsurePoolInternal(gameState, playerId, turnNumber, CollectReservedNames(gameState)).Pool;
        var optionSnapshot = snapshot.Options.FirstOrDefault(o => o.OptionId == optionId);
        return new RecruitmentHireResult(status, snapshot, optionSnapshot, null, sectorId, message);
    }

    private RecruitmentDeclineResult FailureDecline(GameState gameState, Guid playerId, int turnNumber, RecruitmentActionStatus status, Guid optionId, string message)
    {
        var snapshot = EnsurePoolInternal(gameState, playerId, turnNumber, CollectReservedNames(gameState)).Pool;
        var optionSnapshot = snapshot.Options.FirstOrDefault(o => o.OptionId == optionId);
        return new RecruitmentDeclineResult(status, snapshot, optionSnapshot, message);
    }
}
