using System;

namespace ChaosOverlords.Core.Services;

/// <summary>
/// XorShift128+ based deterministic RNG implementation that supports state capture/restore.
/// </summary>
public sealed class DeterministicRngService : IRngService
{
    private ulong _state0;
    private ulong _state1;

    public int Seed { get; private set; }

    public bool IsInitialised { get; private set; }

    public void Reset(int seed)
    {
        Seed = seed;
        var mixer = new SplitMix64(unchecked((ulong)(seed == 0 ? 1 : seed)));
        _state0 = mixer.Next();
        _state1 = mixer.Next();

        if (_state0 == 0 && _state1 == 0)
        {
            _state1 = 1;
        }

        IsInitialised = true;
    }

    public int NextInt()
    {
        EnsureInitialised();
        return unchecked((int)NextUInt64());
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        EnsureInitialised();

        if (minInclusive >= maxExclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExclusive), maxExclusive, "maxExclusive must be greater than minInclusive.");
        }

        var range = (ulong)(maxExclusive - minInclusive);
        var value = NextUInt64() % range;
        return (int)(value + (ulong)minInclusive);
    }

    public double NextDouble()
    {
        EnsureInitialised();
        return (NextUInt64() >> 11) * (1.0 / (1UL << 53));
    }

    private ulong NextUInt64()
    {
        var s1 = _state0;
        var s0 = _state1;
        _state0 = s0;
        s1 ^= s1 << 23;
        s1 ^= s1 >> 17;
        s1 ^= s0;
        s1 ^= s0 >> 26;
        _state1 = s1;
        return _state0 + _state1;
    }

    private void EnsureInitialised()
    {
        if (!IsInitialised)
        {
            throw new InvalidOperationException("RNG has not been initialised with a seed.");
        }
    }

    private struct SplitMix64
    {
        private ulong _state;

        public SplitMix64(ulong seed) => _state = seed;

        public ulong Next()
        {
            var result = unchecked(_state += 0x9E3779B97F4A7C15UL);
            result = (result ^ (result >> 30)) * 0xBF58476D1CE4E5B9UL;
            result = (result ^ (result >> 27)) * 0x94D049BB133111EBUL;
            return result ^ (result >> 31);
        }
    }
}
