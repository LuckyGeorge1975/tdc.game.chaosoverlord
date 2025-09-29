using System;
using System.Collections.Generic;

namespace ChaosOverlords.Core.Domain.Game.Recruitment;

/// <summary>
/// Immutable projection of a recruitment option used by the UI and logging components.
/// </summary>
public sealed record RecruitmentOptionSnapshot(
    Guid OptionId,
    int SlotIndex,
    string GangName,
    int HiringCost,
    int UpkeepCost,
    RecruitmentOptionState State)
{
    public bool IsAvailable => State == RecruitmentOptionState.Available;
    public bool IsDeclined => State == RecruitmentOptionState.Declined;
    public bool IsHired => State == RecruitmentOptionState.Hired;
}

/// <summary>
/// Represents the recruitment options exposed to a single player.
/// </summary>
public sealed record RecruitmentPoolSnapshot(Guid PlayerId, string PlayerName, IReadOnlyList<RecruitmentOptionSnapshot> Options);

/// <summary>
/// Captures the outcome of refreshing recruitment pools at the start of a turn.
/// </summary>
public sealed record RecruitmentRefreshResult(RecruitmentPoolSnapshot Pool, bool HasChanges);

/// <summary>
/// Indicates whether a recruitment action succeeded or why it failed.
/// </summary>
public enum RecruitmentActionStatus
{
    Success,
    InvalidOption,
    AlreadyResolved,
    InvalidSector,
    InsufficientFunds
}

/// <summary>
/// Result payload returned after attempting to hire a gang.
/// </summary>
public sealed record RecruitmentHireResult(
    RecruitmentActionStatus Status,
    RecruitmentPoolSnapshot Pool,
    RecruitmentOptionSnapshot? Option,
    Guid? GangId,
    string? SectorId,
    string? FailureReason);

/// <summary>
/// Result payload returned after declining a candidate.
/// </summary>
public sealed record RecruitmentDeclineResult(
    RecruitmentActionStatus Status,
    RecruitmentPoolSnapshot Pool,
    RecruitmentOptionSnapshot? Option,
    string? FailureReason);
