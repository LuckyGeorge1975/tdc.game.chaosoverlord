using ChaosOverlords.Core.Domain.Players;

namespace ChaosOverlords.Tests.Domain.Players;

public sealed class PlayerTests
{
    [Fact]
    public void Credit_IncreasesCash()
    {
        var player = new Player(Guid.NewGuid(), "Player", 10);

        player.Credit(5);

        Assert.Equal(15, player.Cash);
    }

    [Fact]
    public void Debit_DecreasesCash()
    {
        var player = new Player(Guid.NewGuid(), "Player", 10);

        player.Debit(4);

        Assert.Equal(6, player.Cash);
    }

    [Fact]
    public void AssignGang_AddsUniqueId()
    {
        var player = new Player(Guid.NewGuid(), "Player");
        var gangId = Guid.NewGuid();

        player.AssignGang(gangId);
        player.AssignGang(gangId);

        Assert.Equal([gangId], player.GangIds);
    }
}