using BookHive.Backend.Repositories;
using BookHive.Models;

namespace BookHive.Services;

public class FollowService
{
    private readonly IFollowRepository _followRepository;

    public FollowService(IFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    // following someone you already follow does nothing (no error)
    public async Task FollowAsync(int followerId, int followeeId)
    {
        if (followerId == followeeId)
        {
            throw new ArgumentException("You cannot follow yourself.");
        }

        if (!await _followRepository.UserExistsAsync(followeeId))
        {
            throw new KeyNotFoundException("User was not found.");
        }

        if (await _followRepository.IsFollowingAsync(followerId, followeeId))
        {
            return;
        }

        _followRepository.Add(followerId, followeeId);
        await _followRepository.SaveAsync();
    }

    // unfollowing someone you do not follow does nothing (no error)
    public async Task UnfollowAsync(int followerId, int followeeId)
    {
        bool removed = await _followRepository.RemoveAsync(followerId, followeeId);

        if (removed)
        {
            await _followRepository.SaveAsync();
        }
    }

    public async Task<bool> IsFollowingAsync(int followerId, int followeeId)
    {
        return await _followRepository.IsFollowingAsync(followerId, followeeId);
    }

    public async Task<List<User>> GetFollowersAsync(int userId)
    {
        await EnsureUserExistsAsync(userId);
        return await _followRepository.GetFollowersAsync(userId);
    }

    public async Task<List<User>> GetFollowingAsync(int userId)
    {
        await EnsureUserExistsAsync(userId);
        return await _followRepository.GetFollowingAsync(userId);
    }

    public async Task<(int Followers, int Following)> GetCountsAsync(int userId)
    {
        await EnsureUserExistsAsync(userId);

        int followers = await _followRepository.CountFollowersAsync(userId);
        int following = await _followRepository.CountFollowingAsync(userId);

        return (followers, following);
    }

    private async Task EnsureUserExistsAsync(int userId)
    {
        if (!await _followRepository.UserExistsAsync(userId))
        {
            throw new KeyNotFoundException("User was not found.");
        }
    }
}