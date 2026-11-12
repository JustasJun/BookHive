using BookHive.Backend.Data;
using BookHive.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHive.Backend.Repositories;

public class FollowRepository : IFollowRepository
{
    private const string JoinTable = "Follows";

    private readonly BookHiveDbContext _context;

    public FollowRepository(BookHiveDbContext context)
    {
        _context = context;
    }

    private DbSet<Dictionary<string, object>> Follows =>
        _context.Set<Dictionary<string, object>>(JoinTable);

    public async Task<bool> UserExistsAsync(int userId)
    {
        return await _context.Users.AnyAsync(user => user.Id == userId);
    }

    public async Task<bool> IsFollowingAsync(int followerId, int followeeId)
    {
        return await _context.Users.AnyAsync(user =>
            user.Id == followerId &&
            user.Following.Any(followed => followed.Id == followeeId));
    }

    public void Add(int followerId, int followeeId)
    {
        Follows.Add(new Dictionary<string, object>
        {
            ["FollowerId"] = followerId,
            ["FolloweeId"] = followeeId,
            ["FollowedAt"] = DateTime.UtcNow
        });
    }

    // returns false when the follow did not exist
    public async Task<bool> RemoveAsync(int followerId, int followeeId)
    {
        // key order is the same as in HasKey: FollowerId, FolloweeId
        Dictionary<string, object>? follow =
            await Follows.FindAsync(followerId, followeeId);

        if (follow == null)
        {
            return false;
        }

        Follows.Remove(follow);
        return true;
    }

    public async Task<List<User>> GetFollowersAsync(int userId)
    {
        return await _context.Users
            .Where(user => user.Id == userId)
            .SelectMany(user => user.Followers)
            .OrderBy(follower => follower.Username)
            .ToListAsync();
    }

    public async Task<List<User>> GetFollowingAsync(int userId)
    {
        return await _context.Users
            .Where(user => user.Id == userId)
            .SelectMany(user => user.Following)
            .OrderBy(followed => followed.Username)
            .ToListAsync();
    }

    public async Task<int> CountFollowersAsync(int userId)
    {
        return await _context.Users
            .Where(user => user.Id == userId)
            .SelectMany(user => user.Followers)
            .CountAsync();
    }

    public async Task<int> CountFollowingAsync(int userId)
    {
        return await _context.Users
            .Where(user => user.Id == userId)
            .SelectMany(user => user.Following)
            .CountAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}