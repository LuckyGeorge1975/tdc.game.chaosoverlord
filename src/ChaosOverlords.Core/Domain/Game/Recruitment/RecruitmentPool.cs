using System;
using System.Collections.Generic;

namespace ChaosOverlords.Core.Domain.Game.Recruitment;

/// <summary>
/// Maintains the recruitment options for a single player.
/// </summary>
internal sealed class RecruitmentPool
{
    private readonly RecruitmentOption[] _options;

    public RecruitmentPool(int slotCount, Func<int, RecruitmentOption> optionFactory)
    {
        if (slotCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slotCount), slotCount, "Slot count must be positive.");
        }

        if (optionFactory is null)
        {
            throw new ArgumentNullException(nameof(optionFactory));
        }

        _options = new RecruitmentOption[slotCount];
        for (var i = 0; i < slotCount; i++)
        {
            _options[i] = optionFactory(i);
        }
    }

    public IReadOnlyList<RecruitmentOption> Options => _options;

    public RecruitmentOption GetOption(Guid optionId)
    {
        var option = Array.Find(_options, o => o.Id == optionId);
        return option ?? throw new InvalidOperationException($"Recruitment option '{optionId}' was not found.");
    }

    public bool TryGetOption(Guid optionId, out RecruitmentOption? option)
    {
        option = Array.Find(_options, o => o.Id == optionId);
        return option is not null;
    }

    public void ReplaceOption(int slotIndex, RecruitmentOption option)
    {
        if (slotIndex < 0 || slotIndex >= _options.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(slotIndex), slotIndex, "Slot index is out of bounds.");
        }

        option = option ?? throw new ArgumentNullException(nameof(option));
        _options[slotIndex] = option;
    }
}
