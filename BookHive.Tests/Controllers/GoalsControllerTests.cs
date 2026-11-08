using BookHive.Backend.Controllers;
using BookHive.Backend.DTOs;
using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Services;
using BookHive.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Tests.Controllers;

public class GoalsControllerTests
{
    private const int UserId = 1;

    private readonly IGoalRepository _goalRepository =
        Substitute.For<IGoalRepository>();

    private readonly ILibraryRepository _libraryRepository =
        Substitute.For<ILibraryRepository>();

    private GoalsController CreateController(int? userId = UserId)
    {
        var service = new GoalService(_goalRepository, _libraryRepository);

        return new GoalsController(service).WithUser(userId);
    }

    [Fact]
    public async Task GetGoal_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        ActionResult<ReadingGoal> result =
            await CreateController(userId: null).GetGoal();

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetGoal_WhenUserHasNoGoal_ReturnsNotFound()
    {
        _goalRepository.GetLatestByUserIdAsync(UserId).Returns((ReadingGoal?)null);

        ActionResult<ReadingGoal> result = await CreateController().GetGoal();

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetGoal_WhenGoalExists_ReturnsIt()
    {
        var goal = new ReadingGoal(UserId, GoalPeriod.OneMonth, 10);
        _goalRepository.GetLatestByUserIdAsync(UserId).Returns(goal);

        ActionResult<ReadingGoal> result = await CreateController().GetGoal();

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(goal, ok.Value);
    }

    [Fact]
    public async Task SetGoal_WhenTargetIsNotPositive_ReturnsBadRequest()
    {
        IActionResult result = await CreateController()
            .SetGoal(new SetGoalRequest(GoalPeriod.OneMonth, 0));

        Assert.IsType<BadRequestObjectResult>(result);
        _goalRepository.DidNotReceive().Add(Arg.Any<ReadingGoal>());
    }

    [Fact]
    public async Task SetGoal_WhenValid_AddsGoalSavesAndReturnsIt()
    {
        var savedGoal = new ReadingGoal(UserId, GoalPeriod.OneMonth, 10);
        _goalRepository.GetLatestByUserIdAsync(UserId)
            .Returns((ReadingGoal?)null, savedGoal);

        IActionResult result = await CreateController()
            .SetGoal(new SetGoalRequest(GoalPeriod.OneMonth, 10));

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(savedGoal, ok.Value);
        _goalRepository.Received(1).Add(Arg.Is<ReadingGoal>(goal =>
            goal.UserId == UserId &&
            goal.TargetBooks == 10 &&
            goal.Period == GoalPeriod.OneMonth));
        await _goalRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task GetProgress_WhenUserHasNoGoal_ReturnsNotFound()
    {
        _goalRepository.GetLatestByUserIdAsync(UserId).Returns((ReadingGoal?)null);

        ActionResult<GoalProgress> result = await CreateController().GetProgress();

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetHistory_ReturnsFinishedGoals()
    {
        var finished = new ReadingGoal(UserId, GoalPeriod.OneMonth, 5)
        {
            Status = GoalStatus.Completed,
            BooksRead = 5,
            FinishedAt = DateTime.UtcNow
        };
        _goalRepository.GetLatestByUserIdAsync(UserId).Returns((ReadingGoal?)null);
        _goalRepository.GetFinishedByUserIdAsync(UserId)
            .Returns(new List<ReadingGoal> { finished });

        ActionResult<List<GoalHistoryItem>> result =
            await CreateController().GetHistory();

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
        var history = Assert.IsType<List<GoalHistoryItem>>(ok.Value);
        Assert.Single(history);
    }
}