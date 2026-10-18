using BookHive.Backend.Repositories;
using BookHive.Models;

namespace BookHive.Services;

public class GoalService
{
    private readonly IGoalRepository _goalRepository;
    private readonly ILibraryRepository _libraryRepository;

    public GoalService(IGoalRepository goalRepository, ILibraryRepository libraryRepository)
    {
        _goalRepository = goalRepository;
        _libraryRepository = libraryRepository;
    }

    public ReadingGoal? GetGoal(int userId, DateTime now)
    {
        ReadingGoal? goal = _goalRepository.GetLatestByUserId(userId);

        if (goal != null)
        {
            RefreshStatus(goal, now);
        }

        return goal;
    }

    public void SetGoal(int userId, GoalPeriod period, int targetBooks, DateTime now)
    {
        if (targetBooks <= 0)
        {
            throw new ArgumentException("Target must be positive", nameof(targetBooks));
        }

        if (!Enum.IsDefined(period))
        {
            throw new ArgumentException("Unknown goal period", nameof(period));
        }

        // refreshes status first, so a goal that just ended is saved to history
        ReadingGoal? latest = GetGoal(userId, now);

        if (latest != null && latest.Status == GoalStatus.Active)
        {
            // when changing an active goal, the counting is reset
            latest.Period = period;
            latest.TargetBooks = targetBooks;
            latest.StartedAt = now;
            latest.EndsAt = period.AddTo(now);
        }
        else
        {
            _goalRepository.Add(new ReadingGoal(userId, period, targetBooks));
        }

        _goalRepository.Save();
    }

    public GoalProgress? GetProgress(int userId, DateTime now)
    {
        ReadingGoal? goal = GetGoal(userId, now);

        if (goal == null)
        {
            return null;
        }

        int booksRead = goal.Status == GoalStatus.Active
            ? CountBooksRead(goal)
            : goal.BooksRead;

        return Calculate(goal, booksRead, now);
    }

    public List<GoalHistoryItem> GetHistory(int userId, DateTime now)
    {
        GetGoal(userId, now); // makes sure the latest goal is finalized if it should be

        return _goalRepository.GetFinishedByUserId(userId)
            .Select(goal => new GoalHistoryItem(
                goal.Id,
                goal.Period,
                goal.TargetBooks,
                goal.BooksRead,
                goal.Status,
                goal.StartedAt,
                goal.FinishedAt ?? goal.StartedAt))
            .ToList();
    }

    private int CountBooksRead(ReadingGoal goal)
    {
        return _libraryRepository.CountReadBooks(goal.UserId, goal.StartedAt, goal.EndsAt);
    }

    // An active goal becomes Completed (target reached) or Missed (time is over).
    // The result is saved, so the history doesn't change later.
    private void RefreshStatus(ReadingGoal goal, DateTime now)
    {
        if (goal.Status != GoalStatus.Active)
        {
            return;
        }

        int booksRead = CountBooksRead(goal);
        bool reached = booksRead >= goal.TargetBooks;
        bool expired = now >= goal.EndsAt;

        if (!reached && !expired)
        {
            return;
        }

        goal.Status = reached ? GoalStatus.Completed : GoalStatus.Missed;
        goal.BooksRead = booksRead;
        goal.FinishedAt = now < goal.EndsAt ? now : goal.EndsAt;

        _goalRepository.Save();
    }

    public GoalProgress Calculate(ReadingGoal goal, int booksRead, DateTime now)
    {
        double totalDays = Math.Max(1, (goal.EndsAt - goal.StartedAt).TotalDays);
        double elapsedDays = Math.Clamp((now - goal.StartedAt).TotalDays, 0, totalDays);

        int daysLeft = Math.Max(0, (int)Math.Ceiling((goal.EndsAt - now).TotalDays));
        int expectedByNow = (int)(goal.TargetBooks * elapsedDays / totalDays);
        double percentage = Math.Min(100, booksRead * 100.0 / goal.TargetBooks);

        return new GoalProgress(
            goal.Period,
            goal.StartedAt,
            goal.EndsAt,
            daysLeft,
            goal.TargetBooks,
            booksRead,
            percentage,
            expectedByNow,
            booksRead - expectedByNow,
            booksRead >= goal.TargetBooks,
            goal.Status);
    }
}
