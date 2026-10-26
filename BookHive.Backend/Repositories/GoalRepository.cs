using BookHive.Backend.Data;
using BookHive.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Backend.Repositories;

public class GoalRepository : IGoalRepository
{
    private readonly BookHiveDbContext _context;

    public GoalRepository(BookHiveDbContext context)
    {
        _context = context;
    }

    public async Task<ReadingGoal?> GetLatestByUserIdAsync(int userId)
    {
        return await _context.ReadingGoals
            .Where(goal => goal.UserId == userId)
            .OrderByDescending(goal => goal.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ReadingGoal>> GetFinishedByUserIdAsync(int userId)
    {
        return await _context.ReadingGoals
            .Where(goal => goal.UserId == userId &&
                           goal.Status != GoalStatus.Active)
            .OrderByDescending(goal => goal.FinishedAt)
            .ToListAsync();
    }

    public void Add(ReadingGoal goal)
    {
        _context.ReadingGoals.Add(goal);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}