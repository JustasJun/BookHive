using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Services;

namespace BookHive.Tests.Services;

public class GoalServiceTests
{
    private const int UserId = 1;

    private static readonly DateTime Start =
        new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly IGoalRepository _goalRepository =
        Substitute.For<IGoalRepository>();

    private readonly ILibraryRepository _libraryRepository =
        Substitute.For<ILibraryRepository>();

    private readonly GoalService _service;

    public GoalServiceTests()
    {
        _service = new GoalService(_goalRepository, _libraryRepository);
    }


    [Fact]
    public async Task GetGoalAsync_WhenUserHasNoGoal_ReturnsNullAndDoesNotSave()
    {
        ReturnsLatestGoal(null);

        ReadingGoal? result = await _service.GetGoalAsync(UserId, Start);

        Assert.Null(result);
        await _goalRepository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task GetGoalAsync_WhenTargetIsReached_MarksGoalCompletedAndSaves()
    {
        ReadingGoal goal = CreateGoal(targetBooks: 3);
        DateTime now = Start.AddDays(5);
        ReturnsLatestGoal(goal);
        ReturnsBooksRead(3);

        ReadingGoal? result = await _service.GetGoalAsync(UserId, now);

        Assert.NotNull(result);
        Assert.Equal(GoalStatus.Completed, result.Status);
        Assert.Equal(3, result.BooksRead);
        Assert.Equal(now, result.FinishedAt);
        await _goalRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task GetGoalAsync_WhenTimeIsOverAndTargetNotReached_MarksGoalMissed()
    {
        ReadingGoal goal = CreateGoal(targetBooks: 10);
        DateTime now = goal.EndsAt.AddDays(1);
        ReturnsLatestGoal(goal);
        ReturnsBooksRead(4);

        ReadingGoal? result = await _service.GetGoalAsync(UserId, now);

        Assert.NotNull(result);
        Assert.Equal(GoalStatus.Missed, result.Status);
        Assert.Equal(4, result.BooksRead);
        // FinishedAt is capped at the end of the goal, not "now".
        Assert.Equal(goal.EndsAt, result.FinishedAt);
        await _goalRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task GetGoalAsync_WhenGoalIsStillRunning_StaysActiveAndDoesNotSave()
    {
        ReadingGoal goal = CreateGoal(targetBooks: 10);
        ReturnsLatestGoal(goal);
        ReturnsBooksRead(2);

        ReadingGoal? result = await _service.GetGoalAsync(UserId, Start.AddDays(3));

        Assert.NotNull(result);
        Assert.Equal(GoalStatus.Active, result.Status);
        Assert.Null(result.FinishedAt);
        await _goalRepository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task GetGoalAsync_WhenGoalAlreadyFinished_DoesNotRecountBooks()
    {
        ReadingGoal goal = CreateGoal();
        goal.Status = GoalStatus.Completed;
        ReturnsLatestGoal(goal);

        await _service.GetGoalAsync(UserId, Start.AddDays(3));

        await _libraryRepository.DidNotReceive().CountReadBooksAsync(
            Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<DateTime>());
        await _goalRepository.DidNotReceive().SaveAsync();
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task SetGoalAsync_WhenTargetIsNotPositive_ThrowsArgumentException(
        int targetBooks)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SetGoalAsync(UserId, GoalPeriod.OneMonth, targetBooks, Start));

        Assert.Equal("targetBooks", exception.ParamName);
        _goalRepository.DidNotReceive().Add(Arg.Any<ReadingGoal>());
    }

    [Fact]
    public async Task SetGoalAsync_WhenPeriodIsUnknown_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SetGoalAsync(UserId, (GoalPeriod)99, 5, Start));

        Assert.Equal("period", exception.ParamName);
        _goalRepository.DidNotReceive().Add(Arg.Any<ReadingGoal>());
    }

    [Fact]
    public async Task SetGoalAsync_WhenUserHasNoGoal_AddsNewGoalAndSaves()
    {
        ReturnsLatestGoal(null);

        await _service.SetGoalAsync(UserId, GoalPeriod.ThreeMonths, 12, Start);

        _goalRepository.Received(1).Add(Arg.Is<ReadingGoal>(goal =>
            goal.UserId == UserId
            && goal.Period == GoalPeriod.ThreeMonths
            && goal.TargetBooks == 12));
        await _goalRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task SetGoalAsync_WhenGoalIsActive_UpdatesItAndResetsCounting()
    {
        ReadingGoal goal = CreateGoal(GoalPeriod.OneMonth, targetBooks: 10);
        DateTime now = Start.AddDays(3);
        ReturnsLatestGoal(goal);
        ReturnsBooksRead(1);

        await _service.SetGoalAsync(UserId, GoalPeriod.OneWeek, 5, now);

        Assert.Equal(GoalPeriod.OneWeek, goal.Period);
        Assert.Equal(5, goal.TargetBooks);
        Assert.Equal(now, goal.StartedAt);
        Assert.Equal(now.AddDays(7), goal.EndsAt);
        _goalRepository.DidNotReceive().Add(Arg.Any<ReadingGoal>());
        await _goalRepository.Received().SaveAsync();
    }

    [Fact]
    public async Task SetGoalAsync_WhenLatestGoalIsFinished_AddsNewGoalInsteadOfEditing()
    {
        ReadingGoal oldGoal = CreateGoal(targetBooks: 3);
        oldGoal.Status = GoalStatus.Completed;
        oldGoal.BooksRead = 3;
        ReturnsLatestGoal(oldGoal);

        await _service.SetGoalAsync(UserId, GoalPeriod.OneYear, 20, Start.AddDays(10));

        _goalRepository.Received(1).Add(Arg.Is<ReadingGoal>(goal =>
            goal.Period == GoalPeriod.OneYear && goal.TargetBooks == 20));
        // The old goal stays untouched, so the history doesn't change.
        Assert.Equal(3, oldGoal.TargetBooks);
        Assert.Equal(GoalStatus.Completed, oldGoal.Status);
    }

    [Fact]
    public async Task SetGoalAsync_WhenActiveGoalJustExpired_SavesItAsMissedAndAddsNewGoal()
    {
        ReadingGoal expiredGoal = CreateGoal(GoalPeriod.OneWeek, targetBooks: 10);
        ReturnsLatestGoal(expiredGoal);
        ReturnsBooksRead(0);

        await _service.SetGoalAsync(
            UserId, GoalPeriod.OneMonth, 4, expiredGoal.EndsAt.AddDays(1));

        Assert.Equal(GoalStatus.Missed, expiredGoal.Status);
        _goalRepository.Received(1).Add(Arg.Any<ReadingGoal>());
    }


    [Fact]
    public async Task GetProgressAsync_WhenUserHasNoGoal_ReturnsNull()
    {
        ReturnsLatestGoal(null);

        GoalProgress? progress = await _service.GetProgressAsync(UserId, Start);

        Assert.Null(progress);
    }

    [Fact]
    public async Task GetProgressAsync_WhenGoalIsActive_UsesLiveBookCount()
    {
        ReadingGoal goal = CreateGoal(targetBooks: 10);
        ReturnsLatestGoal(goal);
        ReturnsBooksRead(4);

        GoalProgress? progress =
            await _service.GetProgressAsync(UserId, Start.AddDays(3));

        Assert.NotNull(progress);
        Assert.Equal(4, progress.BooksRead);
        Assert.Equal(GoalStatus.Active, progress.Status);
    }

    [Fact]
    public async Task GetProgressAsync_WhenGoalIsFinished_UsesFrozenBookCount()
    {
        ReadingGoal goal = CreateGoal(targetBooks: 10);
        goal.Status = GoalStatus.Missed;
        goal.BooksRead = 7;
        ReturnsLatestGoal(goal);

        GoalProgress? progress =
            await _service.GetProgressAsync(UserId, goal.EndsAt.AddDays(30));

        Assert.NotNull(progress);
        Assert.Equal(7, progress.BooksRead);
        await _libraryRepository.DidNotReceive().CountReadBooksAsync(
            Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<DateTime>());
    }


    [Fact]
    public void Calculate_WhenHalfOfTimePassed_ComputesPaceAndPercentage()
    {
        ReadingGoal goal = CreateGoalWithLength(days: 10, targetBooks: 10);

        GoalProgress progress = _service.Calculate(goal, booksRead: 3, Start.AddDays(5));

        Assert.Equal(5, progress.DaysLeft);
        Assert.Equal(5, progress.ExpectedByNow);
        Assert.Equal(-2, progress.PaceDifference);   // 3 read, 5 expected: behind
        Assert.Equal(30.0, progress.Percentage, precision: 5);
        Assert.False(progress.IsCompleted);
    }

    [Fact]
    public void Calculate_WhenAheadOfSchedule_PaceDifferenceIsPositive()
    {
        ReadingGoal goal = CreateGoalWithLength(days: 10, targetBooks: 10);

        GoalProgress progress = _service.Calculate(goal, booksRead: 8, Start.AddDays(5));

        Assert.Equal(3, progress.PaceDifference);
    }

    [Fact]
    public void Calculate_WhenMoreBooksReadThanTarget_CapsPercentageAtHundred()
    {
        ReadingGoal goal = CreateGoalWithLength(days: 10, targetBooks: 10);

        GoalProgress progress = _service.Calculate(goal, booksRead: 15, Start.AddDays(2));

        Assert.Equal(100.0, progress.Percentage, precision: 5);
        Assert.True(progress.IsCompleted);
    }

    [Fact]
    public void Calculate_WhenGoalHasEnded_HasNoDaysLeftAndExpectsWholeTarget()
    {
        ReadingGoal goal = CreateGoalWithLength(days: 10, targetBooks: 10);

        GoalProgress progress = _service.Calculate(goal, booksRead: 4, Start.AddDays(13));

        Assert.Equal(0, progress.DaysLeft);
        Assert.Equal(10, progress.ExpectedByNow);
    }

    [Fact]
    public void Calculate_WhenNowIsBeforeStart_ExpectsNothingYet()
    {
        ReadingGoal goal = CreateGoalWithLength(days: 10, targetBooks: 10);

        GoalProgress progress = _service.Calculate(goal, booksRead: 0, Start.AddDays(-2));

        Assert.Equal(0, progress.ExpectedByNow);
    }

    

    [Fact]
    public async Task GetHistoryAsync_MapsFinishedGoalsToHistoryItems()
    {
        ReadingGoal completed = CreateGoal(GoalPeriod.OneMonth, targetBooks: 5);
        completed.Id = 7;
        completed.Status = GoalStatus.Completed;
        completed.BooksRead = 5;
        completed.FinishedAt = Start.AddDays(20);

        ReturnsLatestGoal(null);
        _goalRepository.GetFinishedByUserIdAsync(UserId)
            .Returns(new List<ReadingGoal> { completed });

        List<GoalHistoryItem> history = await _service.GetHistoryAsync(UserId, Start.AddDays(40));

        GoalHistoryItem item = Assert.Single(history);
        Assert.Equal(7, item.Id);
        Assert.Equal(GoalPeriod.OneMonth, item.Period);
        Assert.Equal(5, item.TargetBooks);
        Assert.Equal(5, item.BooksRead);
        Assert.Equal(GoalStatus.Completed, item.Status);
        Assert.Equal(Start, item.StartedAt);
        Assert.Equal(Start.AddDays(20), item.FinishedAt);
    }

    [Fact]
    public async Task GetHistoryAsync_WhenFinishedAtIsMissing_FallsBackToStartedAt()
    {
        ReadingGoal goal = CreateGoal();
        goal.Status = GoalStatus.Missed;
        goal.FinishedAt = null;

        ReturnsLatestGoal(null);
        _goalRepository.GetFinishedByUserIdAsync(UserId)
            .Returns(new List<ReadingGoal> { goal });

        List<GoalHistoryItem> history = await _service.GetHistoryAsync(UserId, Start.AddDays(40));

        Assert.Equal(Start, Assert.Single(history).FinishedAt);
    }

    [Fact]
    public async Task GetHistoryAsync_FinalizesExpiredLatestGoalBeforeReadingHistory()
    {
        ReadingGoal expired = CreateGoal(GoalPeriod.OneWeek, targetBooks: 10);
        ReturnsLatestGoal(expired);
        ReturnsBooksRead(2);
        _goalRepository.GetFinishedByUserIdAsync(UserId)
            .Returns(new List<ReadingGoal>());

        await _service.GetHistoryAsync(UserId, expired.EndsAt.AddDays(1));

        Assert.Equal(GoalStatus.Missed, expired.Status);
        await _goalRepository.Received(1).SaveAsync();
    }


    private static ReadingGoal CreateGoal(
        GoalPeriod period = GoalPeriod.OneMonth,
        int targetBooks = 10)
    {
        // The constructor uses DateTime.UtcNow, so the dates are fixed here
        // to make the tests deterministic.
        var goal = new ReadingGoal(UserId, period, targetBooks)
        {
            StartedAt = Start,
            EndsAt = period.AddTo(Start)
        };

        return goal;
    }

    private static ReadingGoal CreateGoalWithLength(int days, int targetBooks)
    {
        ReadingGoal goal = CreateGoal(targetBooks: targetBooks);
        goal.EndsAt = Start.AddDays(days);

        return goal;
    }

    private void ReturnsLatestGoal(ReadingGoal? goal) =>
        _goalRepository.GetLatestByUserIdAsync(UserId).Returns(goal);

    private void ReturnsBooksRead(int count) =>
        _libraryRepository
            .CountReadBooksAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(count);
}