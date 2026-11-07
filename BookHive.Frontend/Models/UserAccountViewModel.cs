using BookHive.Models;
using BookHive.Frontend.DTOs;

namespace BookHive.Frontend.Models;

public class UserAccountViewModel
{
    public required UserProfileResponse Profile { get; init; }
    public int BookCount { get; init; }
    public int FollowersCount { get; init; }
    public int FollowingCount { get; init; }
    public required List<Review> Reviews { get; init; }
    public bool IsOwner { get; init; } = true;
    public bool IsFollowing { get; init; }
}
