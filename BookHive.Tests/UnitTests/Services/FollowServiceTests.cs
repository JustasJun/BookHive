using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Services;

namespace BookHive.Tests.UnitTests.Services;

public class FollowServiceTests
{
    private const int FollowerId = 1;
    private const int FolloweeId = 2;

    private readonly IFollowRepository _followRepositorySubstitute =
        Substitute.For<IFollowRepository>();

    private readonly FollowService _service;

    public FollowServiceTests()
    {
        _service = new FollowService(_followRepositorySubstitute);
    }

    [Fact]
    public async Task FollowAsync_WhenFollowingYourself_ThrowsAndDoesNotSave()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.FollowAsync(FollowerId, FollowerId));

        _followRepositorySubstitute.DidNotReceive().Add(Arg.Any<int>(), Arg.Any<int>());
        await _followRepositorySubstitute.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task FollowAsync_WhenUserDoesNotExist_ThrowsNotFound()
    {
        _followRepositorySubstitute.UserExistsAsync(FolloweeId).Returns(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.FollowAsync(FollowerId, FolloweeId));

        await _followRepositorySubstitute.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task FollowAsync_WhenAlreadyFollowing_DoesNothing()
    {
        _followRepositorySubstitute.UserExistsAsync(FolloweeId).Returns(true);
        _followRepositorySubstitute.IsFollowingAsync(FollowerId, FolloweeId).Returns(true);

        await _service.FollowAsync(FollowerId, FolloweeId);

        _followRepositorySubstitute.DidNotReceive().Add(Arg.Any<int>(), Arg.Any<int>());
        await _followRepositorySubstitute.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task FollowAsync_WhenNotFollowing_AddsAndSaves()
    {
        _followRepositorySubstitute.UserExistsAsync(FolloweeId).Returns(true);
        _followRepositorySubstitute.IsFollowingAsync(FollowerId, FolloweeId).Returns(false);

        await _service.FollowAsync(FollowerId, FolloweeId);

        _followRepositorySubstitute.Received(1).Add(FollowerId, FolloweeId);
        await _followRepositorySubstitute.Received(1).SaveAsync();
    }

    [Fact]
    public async Task UnfollowAsync_WhenFollowing_RemovesAndSaves()
    {
        _followRepositorySubstitute.RemoveAsync(FollowerId, FolloweeId).Returns(true);

        await _service.UnfollowAsync(FollowerId, FolloweeId);

        await _followRepositorySubstitute.Received(1).SaveAsync();
    }

    [Fact]
    public async Task UnfollowAsync_WhenNotFollowing_DoesNotSave()
    {
        _followRepositorySubstitute.RemoveAsync(FollowerId, FolloweeId).Returns(false);

        await _service.UnfollowAsync(FollowerId, FolloweeId);

        await _followRepositorySubstitute.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task GetFollowersAsync_WhenUserDoesNotExist_ThrowsNotFound()
    {
        _followRepositorySubstitute.UserExistsAsync(FolloweeId).Returns(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.GetFollowersAsync(FolloweeId));
    }

    [Fact]
    public async Task GetCountsAsync_ReturnsFollowerAndFollowingCounts()
    {
        _followRepositorySubstitute.UserExistsAsync(FollowerId).Returns(true);
        _followRepositorySubstitute.CountFollowersAsync(FollowerId).Returns(5);
        _followRepositorySubstitute.CountFollowingAsync(FollowerId).Returns(3);

        var (followers, following) = await _service.GetCountsAsync(FollowerId);

        Assert.Equal(5, followers);
        Assert.Equal(3, following);
    }
}
