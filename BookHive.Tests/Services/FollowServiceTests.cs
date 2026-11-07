using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Services;

namespace BookHive.Tests.Services;

public class FollowServiceTests
{
    private const int FollowerId = 1;
    private const int FolloweeId = 2;

    private readonly IFollowRepository _followRepository =
        Substitute.For<IFollowRepository>();

    private readonly FollowService _service;

    public FollowServiceTests()
    {
        _service = new FollowService(_followRepository);
    }

    [Fact]
    public async Task FollowAsync_WhenFollowingYourself_ThrowsAndDoesNotSave()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.FollowAsync(FollowerId, FollowerId));

        _followRepository.DidNotReceive().Add(Arg.Any<int>(), Arg.Any<int>());
        await _followRepository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task FollowAsync_WhenUserDoesNotExist_ThrowsNotFound()
    {
        _followRepository.UserExistsAsync(FolloweeId).Returns(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.FollowAsync(FollowerId, FolloweeId));

        await _followRepository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task FollowAsync_WhenAlreadyFollowing_DoesNothing()
    {
        _followRepository.UserExistsAsync(FolloweeId).Returns(true);
        _followRepository.IsFollowingAsync(FollowerId, FolloweeId).Returns(true);

        await _service.FollowAsync(FollowerId, FolloweeId);

        _followRepository.DidNotReceive().Add(Arg.Any<int>(), Arg.Any<int>());
        await _followRepository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task FollowAsync_WhenNotFollowing_AddsAndSaves()
    {
        _followRepository.UserExistsAsync(FolloweeId).Returns(true);
        _followRepository.IsFollowingAsync(FollowerId, FolloweeId).Returns(false);

        await _service.FollowAsync(FollowerId, FolloweeId);

        _followRepository.Received(1).Add(FollowerId, FolloweeId);
        await _followRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task UnfollowAsync_WhenFollowing_RemovesAndSaves()
    {
        _followRepository.RemoveAsync(FollowerId, FolloweeId).Returns(true);

        await _service.UnfollowAsync(FollowerId, FolloweeId);

        await _followRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task UnfollowAsync_WhenNotFollowing_DoesNotSave()
    {
        _followRepository.RemoveAsync(FollowerId, FolloweeId).Returns(false);

        await _service.UnfollowAsync(FollowerId, FolloweeId);

        await _followRepository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task GetFollowersAsync_WhenUserDoesNotExist_ThrowsNotFound()
    {
        _followRepository.UserExistsAsync(FolloweeId).Returns(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.GetFollowersAsync(FolloweeId));
    }

    [Fact]
    public async Task GetCountsAsync_ReturnsFollowerAndFollowingCounts()
    {
        _followRepository.UserExistsAsync(FollowerId).Returns(true);
        _followRepository.CountFollowersAsync(FollowerId).Returns(5);
        _followRepository.CountFollowingAsync(FollowerId).Returns(3);

        var (followers, following) = await _service.GetCountsAsync(FollowerId);

        Assert.Equal(5, followers);
        Assert.Equal(3, following);
    }

    [Fact]
    public async Task GetFollowingIdsAsync_ReturnsIdsFromRepository()
    {
        _followRepository.GetFollowingIdsAsync(FollowerId)
            .Returns(new List<int> { 2, 3 });

        List<int> ids = await _service.GetFollowingIdsAsync(FollowerId);

        Assert.Equal(new List<int> { 2, 3 }, ids);
    }
}