using BookHive.Models;

namespace BookHive.Services;

public interface IFollowService
{
    Task FollowAsync(int followerId, int followeeId);
    Task UnfollowAsync(int followerId, int followeeId);
    Task<bool> IsFollowingAsync(int followerId, int followeeId);
    Task<List<User>> GetFollowersAsync(int userId);
    Task<List<User>> GetFollowingAsync(int userId);
    Task<(int Followers, int Following)> GetCountsAsync(int userId);
}