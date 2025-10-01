using System;
using ChaosOverlords.Core.Domain.Game.Actions;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// Deterministic random number generator abstraction that can be seeded for reproducible outcomes.
/// </summary>
public interface IRngService
{
    /// <summary>
    /// Current seed used to initialise the generator.
    /// </summary>
    int Seed { get; }

    /// <summary>
    /// Indicates whether the generator has been initialised.
    /// </summary>
    bool IsInitialised { get; }

    /// <summary>
    /// Resets the generator to the provided seed, discarding any previous state.
    /// </summary>
    void Reset(int seed);

    /// <summary>
    /// Returns a uniformly distributed 32-bit integer.
    /// </summary>
    int NextInt();

    /// <summary>
    /// Returns a uniformly distributed integer in the range [minInclusive, maxExclusive).
    /// </summary>
    int NextInt(int minInclusive, int maxExclusive);

    /// <summary>
    /// Returns a double in the range [0, 1).
    /// </summary>
    double NextDouble();

    /// <summary>
    /// Returns a percentile roll (1-100 inclusive).
    /// </summary>
    PercentileRollResult RollPercent();

    /// <summary>
    /// Rolls a configurable number of dice and returns the aggregated result.
    /// </summary>
    DiceRollResult RollDice(int diceCount, int sides, int modifier = 0);
}
