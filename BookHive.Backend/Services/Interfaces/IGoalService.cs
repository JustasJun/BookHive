using BookHive.Models;

namespace BookHive.Services;

public interface IGoalService
{
    Task<ReadingGoal?> GetGoalAsync(int userId, DateTime now);
    Task SetGoalAsync(int userId, GoalPeriod period, int targetBooks, DateTime now);
    Task<GoalProgress?> GetProgressAsync(int userId, DateTime now);
    Task<List<GoalHistoryItem>> GetHistoryAsync(int userId, DateTime now);
}
