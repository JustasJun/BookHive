namespace BookHive.Frontend.DTOs;

public record UserProfileResponse(
    int Id,
    string Username,
    string? DisplayName,
    string? Bio,
    string? AvatarUrl);
