using System;
using ChaosOverlords.Core.GameData;

namespace ChaosOverlords.Core.Domain.Game.Recruitment;

/// <summary>
/// Internal representation of a recruitment candidate exposed to a player for the current turn.
/// </summary>
internal sealed class RecruitmentOption
{
    public RecruitmentOption(int slotIndex, GangData gangData, int createdTurn)
    {
        if (gangData is null)
        {
            throw new ArgumentNullException(nameof(gangData));
        }

        SlotIndex = slotIndex;
        Id = Guid.NewGuid();
        GangData = gangData;
        State = RecruitmentOptionState.Available;
        LastUpdatedTurn = createdTurn;
    }

    public Guid Id { get; private set; }

    public int SlotIndex { get; }

    public GangData GangData { get; private set; }

    public RecruitmentOptionState State { get; private set; }

    public int LastUpdatedTurn { get; private set; }

    public bool CanHire => State == RecruitmentOptionState.Available;

    public bool NeedsRefresh(int currentTurn)
    {
        if (currentTurn <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentTurn), currentTurn, "Turn number must be positive.");
        }

        return State != RecruitmentOptionState.Available && currentTurn > LastUpdatedTurn;
    }

    public void Replace(GangData gangData, int currentTurn)
    {
        if (gangData is null)
        {
            throw new ArgumentNullException(nameof(gangData));
        }

        GangData = gangData;
        Id = Guid.NewGuid();
        State = RecruitmentOptionState.Available;
        LastUpdatedTurn = currentTurn;
    }

    public void MarkDeclined(int currentTurn)
    {
        State = RecruitmentOptionState.Declined;
        LastUpdatedTurn = currentTurn;
    }

    public void MarkHired(int currentTurn)
    {
        State = RecruitmentOptionState.Hired;
        LastUpdatedTurn = currentTurn;
    }
}
