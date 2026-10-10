using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface IGoalRepository
{
    ReadingGoal? GetLatestByUserId(int userId);
    List<ReadingGoal> GetFinishedByUserId(int userId);
    void Add(ReadingGoal goal);
    void Save();
}