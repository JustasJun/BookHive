using BookHive.Models;

namespace BookHive.Frontend.DTOs;

public record UserAccountResponse(
    UserProfileResponse Profile,
    int BookCount,
    int FollowersCount,
    int FollowingCount,
    List<Review> Reviews);
