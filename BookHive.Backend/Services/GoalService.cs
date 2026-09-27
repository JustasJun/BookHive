using BookHive.Models;

namespace BookHive.Services;

public class GoalService
{
    private ReadingGoal? _goal;

    public ReadingGoal? GetGoal()
    {
        return _goal;
    }

    public void SetGoal(int year, int targetBooks)
    {
        if (targetBooks <= 0)
        {
            throw new ArgumentException(nameof(targetBooks));
        }

        _goal = new ReadingGoal(1, year, targetBooks);
    }

    public GoalProgress Calculate(ReadingGoal goal, int booksRead, DateOnly today)
    {
        int daysInYear = DateTime.IsLeapYear(goal.Year) ? 366 : 365;

        int dayOfYear;

        if (goal.Year == today.Year)
        {
            dayOfYear = today.DayOfYear;
        }
        else if (goal.Year < today.Year)
        {
            dayOfYear = daysInYear;
        }
        else
        {
            dayOfYear = 0;
        }

        int expectedByNow = (int)(goal.TargetBooks * (double)dayOfYear / daysInYear);

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