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

    public ReadingGoal? GetByUserId(int userId)
    {
        return _context.ReadingGoals.FirstOrDefault(goal => goal.UserId == userId);
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