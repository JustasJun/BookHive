using BookHive.Models;

namespace BookHive.Backend.DTOs;

// public info only: no email, no password hash
public record UserSummaryResponse(
    int Id,
    string Username,
    string? DisplayName,
    string? AvatarUrl)
{
    public static UserSummaryResponse From(User user) =>
        new(user.Id, user.Username, user.DisplayName, user.AvatarUrl);
}