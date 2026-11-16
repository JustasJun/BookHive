using BookHive.Backend.Data;
using BookHive.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Backend.Repositories;

public class GoalRepository : Repository<ReadingGoal>, IGoalRepository
{
    public GoalRepository(BookHiveDbContext context) : base(context)
    {
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
}