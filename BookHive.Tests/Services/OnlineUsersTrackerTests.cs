using BookHive.Services;
using Xunit;

namespace BookHive.Tests.Services;

public class OnlineUsersTrackerTests
{
    private readonly OnlineUsersTracker _tracker = new();

    [Fact]
    public void OnlineCount_WhenNobodyConnected_IsZero()
    {
        Assert.Equal(0, _tracker.OnlineCount);
    }

    [Fact]
    public void Connect_OneUser_CountsOne()
    {
        _tracker.Connect("c1", 1);

        Assert.Equal(1, _tracker.OnlineCount);
    }

    [Fact]
    public void Connect_SameUserInTwoTabs_CountsUserOnce()
    {
        _tracker.Connect("c1", 1);
        _tracker.Connect("c2", 1);

        Assert.Equal(1, _tracker.OnlineCount);
    }

    [Fact]
    public void Disconnect_OneOfTwoTabs_UserStaysOnline()
    {
        _tracker.Connect("c1", 1);
        _tracker.Connect("c2", 1);

        _tracker.Disconnect("c1");

        Assert.Equal(1, _tracker.OnlineCount);
    }

    [Fact]
    public void Disconnect_LastConnection_UserGoesOffline()
    {
        _tracker.Connect("c1", 1);
        _tracker.Connect("c2", 2);

        _tracker.Disconnect("c1");

        Assert.Equal(1, _tracker.OnlineCount);
    }

    [Fact]
    public void Disconnect_UnknownConnection_DoesNothing()
    {
        _tracker.Connect("c1", 1);

        _tracker.Disconnect("does-not-exist");

        Assert.Equal(1, _tracker.OnlineCount);
    }

    [Fact]
    public void Connect_ManyUsersInParallel_CountsAllOfThem()
    {
        Parallel.For(0, 500, i => _tracker.Connect($"c{i}", i));

        Assert.Equal(500, _tracker.OnlineCount);
    }

    [Fact]
    public void ConnectAndDisconnectInParallel_EndsWithNobodyOnline()
    {
        Parallel.For(0, 500, i =>
        {
            _tracker.Connect($"c{i}", i);
            _tracker.Disconnect($"c{i}");
        });

        Assert.Equal(0, _tracker.OnlineCount);
    }
}