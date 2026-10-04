using BookHive.Backend.Data;
using BookHive.Models;

namespace BookHive.Services;

public class GoalService
{
    private readonly BookHiveDbContext _context;

    public GoalService(BookHiveDbContext context)
    {
        _context = context;
    }

    public ReadingGoal? GetGoal(int userId)
    {
        return _context.ReadingGoals.FirstOrDefault(goal => goal.UserId == userId);
    }

    public void SetGoal(int userId, int year, int targetBooks)
    {
        if (targetBooks <= 0)
        {
            throw new ArgumentException("Target must be positive", nameof(targetBooks));
        }

        ReadingGoal? existing = GetGoal(userId);

        if (existing == null)
        {
            _context.ReadingGoals.Add(new ReadingGoal(userId, year, targetBooks));
        }
        else
        {
            // new goal = fresh start, progress is counted from now
            existing.Year = year;
            existing.TargetBooks = targetBooks;
            existing.StartedAt = DateTime.UtcNow;
        }

        _context.SaveChanges();
    }

    public GoalProgress Calculate(ReadingGoal goal, int booksRead, DateOnly today)
    {
        DateOnly yearStart = new DateOnly(goal.Year, 1, 1);
        DateOnly yearEnd = new DateOnly(goal.Year, 12, 31);

       
        DateOnly startedAt = DateOnly.FromDateTime(goal.StartedAt.ToLocalTime());
        DateOnly start = startedAt > yearStart ? startedAt : yearStart;

        int totalDays = Math.Max(1, yearEnd.DayNumber - start.DayNumber + 1);

        int elapsedDays;

        if (today <= start)
        {
            elapsedDays = 0;
        }
        else if (today >= yearEnd)
        {
            elapsedDays = totalDays;
        }
        else
        {
            elapsedDays = today.DayNumber - start.DayNumber;
        }

        int expectedByNow = (int)(goal.TargetBooks * (double)elapsedDays / totalDays);

        double percentage = Math.Min(100, booksRead * 100.0 / goal.TargetBooks);

        return new GoalProgress(
            goal.Year,
            goal.TargetBooks,
            booksRead,
            percentage,
            expectedByNow,
            booksRead - expectedByNow,
            booksRead >= goal.TargetBooks
        );
    }
}