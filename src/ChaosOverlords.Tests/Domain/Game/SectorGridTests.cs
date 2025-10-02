using ChaosOverlords.Core.Domain.Game;
using Xunit;

namespace ChaosOverlords.Tests.Domain.Game;

public sealed class SectorGridTests
{
    [Theory]
    [InlineData("A1", "A2")]
    [InlineData("A2", "A1")]
    [InlineData("A1", "B1")]
    [InlineData("B1", "A1")]
    [InlineData("C3", "C2")]
    [InlineData("C3", "B3")]
    [InlineData("A1", "B2")] // diagonal
    [InlineData("B2", "A1")] // diagonal
    [InlineData("C3", "B2")] // diagonal
    [InlineData("C3", "D2")] // diagonal
    [InlineData("C3", "B4")] // diagonal
    [InlineData("C3", "D4")] // diagonal
    public void AreAdjacent_ReturnsTrue_ForOrthogonalAndDiagonalNeighbors(string a, string b)
    {
        Assert.True(SectorGrid.AreAdjacent(a, b));
    }

    [Theory]
    [InlineData("A1", "A1")]
    [InlineData("B2", "B2")]
    public void AreAdjacent_ReturnsFalse_ForSameSector(string a, string b)
    {
        Assert.False(SectorGrid.AreAdjacent(a, b));
    }
}
