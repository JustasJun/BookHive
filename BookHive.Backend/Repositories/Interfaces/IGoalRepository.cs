using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface IGoalRepository
{
    Task<ReadingGoal?> GetLatestByUserIdAsync(int userId);
    Task<List<ReadingGoal>> GetFinishedByUserIdAsync(int userId);
    void Add(ReadingGoal goal);
    Task SaveAsync();
}
