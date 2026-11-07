using BookHive.Models;

namespace BookHive.Backend.DTOs;

public record UserAccountResponse(
    UserProfileResponse Profile,
    int BookCount,
    int FollowersCount,
    int FollowingCount,
    List<Review> Reviews);
