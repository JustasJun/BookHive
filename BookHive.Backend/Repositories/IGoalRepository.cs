using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface IGoalRepository
{
    ReadingGoal? GetByUserId(int userId);
    void Add(ReadingGoal goal);
    void Save();
}