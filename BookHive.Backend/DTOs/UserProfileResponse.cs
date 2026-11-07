using BookHive.Models;

namespace BookHive.Backend.DTOs;

public record UserProfileResponse(
    int Id,
    string Username,
    string? DisplayName,
    string? Bio,
    string? AvatarUrl)
{
    public static UserProfileResponse From(User user) =>
        new(user.Id, user.Username, user.DisplayName, user.Bio, user.AvatarUrl);
}
