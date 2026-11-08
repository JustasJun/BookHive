using BookHive.Backend.Controllers;
using BookHive.Backend.DTOs;
using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Tests.Helpers;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Tests.Controllers;

public class FollowsControllerTests
{
    private const int CurrentUserId = 1;
    private const int OtherUserId = 2;

    private readonly IFollowRepository _followRepository =
        Substitute.For<IFollowRepository>();

    private FollowsController CreateController(int? userId = CurrentUserId)
    {
        var service = new FollowService(_followRepository);

        return new FollowsController(service).WithUser(userId);
    }

    [Fact]
    public async Task Follow_WhenNotLoggedIn_ReturnUnauthorized()
    {
        IActionResult result = await CreateController(userId: null).Follow(OtherUserId);

        Assert.IsType<UnauthorizedResult>(result);
        _followRepository.DidNotReceive().Add(Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async Task Follow_WhenFollowingYourself_ReturnBadRequest()
    {
        IActionResult result = await CreateController().Follow(CurrentUserId);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Follow_WhenUserDoesNotExist_ReturnNotFound()
    {
        _followRepository.UserExistsAsync(OtherUserId).Returns(false);

        IActionResult result = await CreateController().Follow(OtherUserId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Follow_WhenValid_ReturnsNoContentAndSaves()
    {
        _followRepository.UserExistsAsync(OtherUserId).Returns(true);
        _followRepository.IsFollowingAsync(CurrentUserId, OtherUserId).Returns(false);

        IActionResult result = await CreateController().Follow(OtherUserId);

        Assert.IsType<NoContentResult>(result);
        _followRepository.Received(1).Add(CurrentUserId, OtherUserId);
        await _followRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task Unfollow_WhenFollowing_ReturnsNoContentAndSaves()
    {
        _followRepository.RemoveAsync(CurrentUserId, OtherUserId).Returns(true);

        IActionResult result = await CreateController().Unfollow(OtherUserId);

        Assert.IsType<NoContentResult>(result);
        await _followRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task IsFollowing_WhenFollowing_ReturnsTrue()
    {
        _followRepository.IsFollowingAsync(CurrentUserId, OtherUserId).Returns(true);

        ActionResult<bool> result = await CreateController().IsFollowing(OtherUserId);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.True(Assert.IsType<bool>(ok.Value));
    }

    [Fact]
    public async Task GetFollowers_WhenUserDoesNotExist_ReturnsNotFound()
    {
        _followRepository.UserExistsAsync(OtherUserId).Returns(false);

        ActionResult<List<UserSummaryResponse>> result =
            await CreateController().GetFollowers(OtherUserId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetFollowers_ReturnsPublicSummaries()
    {
        _followRepository.UserExistsAsync(OtherUserId).Returns(true);
        _followRepository.GetFollowersAsync(OtherUserId).Returns(new List<User>
        {
            new("anna", "anna@test.com", "hash") { Id = 5 }
        });

        ActionResult<List<UserSummaryResponse>> result =
            await CreateController().GetFollowers(OtherUserId);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        var summaries = Assert.IsType<List<UserSummaryResponse>>(ok.Value);
        Assert.Equal("anna", Assert.Single(summaries).Username);
    }

    [Fact]
    public async Task GetCounts_ReturnsFollowerAndFollowingCounts()
    {
        _followRepository.UserExistsAsync(OtherUserId).Returns(true);
        _followRepository.CountFollowersAsync(OtherUserId).Returns(5);
        _followRepository.CountFollowingAsync(OtherUserId).Returns(3);

        ActionResult<FollowCountsResponse> result =
            await CreateController().GetCounts(OtherUserId);

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(new FollowCountsResponse(5, 3), ok.Value);
    }
}
