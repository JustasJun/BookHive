using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface IFollowRepository
{
    Task<bool> UserExistsAsync(int userId);
    Task<bool> IsFollowingAsync(int followerId, int followeeId);
    void Add(int followerId, int followeeId);
    Task<bool> RemoveAsync(int followerId, int followeeId);
    Task<List<User>> GetFollowersAsync(int userId);
    Task<List<User>> GetFollowingAsync(int userId);
    Task<int> CountFollowersAsync(int userId);
    Task<int> CountFollowingAsync(int userId);
    Task SaveAsync();
}