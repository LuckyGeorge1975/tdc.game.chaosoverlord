using ChaosOverlords.Core.Services;

namespace ChaosOverlords.Tests.Services;

public sealed class DeterministicRngServiceTests
{
    [Fact]
    public void Reset_WithSameSeed_YieldsDeterministicSequence()
    {
        var seed = 123456789;
        var first = new DeterministicRngService();
        var second = new DeterministicRngService();

        first.Reset(seed);
        second.Reset(seed);

        var sequenceLength = 10;
        var firstSequence = new int[sequenceLength];
        var secondSequence = new int[sequenceLength];

        for (var i = 0; i < sequenceLength; i++)
        {
            firstSequence[i] = first.NextInt();
            secondSequence[i] = second.NextInt();
        }

        Assert.Equal(firstSequence, secondSequence);
    }

    [Fact]
    public void Reset_WithDifferentSeeds_ProducesDifferentSequences()
    {
        var first = new DeterministicRngService();
        var second = new DeterministicRngService();

        first.Reset(1);
        second.Reset(2);

        var values = Enumerable.Range(0, 5)
            .Select(_ => (A: first.NextInt(), B: second.NextInt()))
            .ToArray();

        Assert.Contains(values, pair => pair.A != pair.B);
    }

    [Fact]
    public void NextInt_WithRange_ReturnsValueWithinBounds()
    {
        var rng = new DeterministicRngService();
        rng.Reset(42);

        var value = rng.NextInt(5, 15);

        Assert.InRange(value, 5, 14);
    }

    [Fact]
    public void NextDouble_ReturnsValueBetweenZeroAndOne()
    {
        var rng = new DeterministicRngService();
        rng.Reset(42);

        var value = rng.NextDouble();

        Assert.True(value >= 0.0 && value < 1.0);
    }

    [Fact]
    public void NextInt_ThrowsWhenSeedNotInitialised()
    {
        var rng = new DeterministicRngService();

        Assert.Throws<InvalidOperationException>(() => rng.NextInt());
    }
}
