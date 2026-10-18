using BookHive.Backend.Data;
using BookHive.Models;

namespace BookHive.Backend.Repositories;

public class GoalRepository : IGoalRepository
{
    private readonly BookHiveDbContext _context;

    public GoalRepository(BookHiveDbContext context)
    {
        _context = context;
    }

    public ReadingGoal? GetLatestByUserId(int userId)
    {
        return _context.ReadingGoals
            .Where(goal => goal.UserId == userId)
            .OrderByDescending(goal => goal.Id)
            .FirstOrDefault();
    }

    public List<ReadingGoal> GetFinishedByUserId(int userId)
    {
        return _context.ReadingGoals
            .Where(goal => goal.UserId == userId && goal.Status != GoalStatus.Active)
            .OrderByDescending(goal => goal.FinishedAt)
            .ToList();
    }

    public void Add(ReadingGoal goal)
    {
        _context.ReadingGoals.Add(goal);
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
