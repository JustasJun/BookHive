using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface IGoalRepository : IRepository<ReadingGoal>
{
    Task<ReadingGoal?> GetLatestByUserIdAsync(int userId);
    Task<List<ReadingGoal>> GetFinishedByUserIdAsync(int userId);
}