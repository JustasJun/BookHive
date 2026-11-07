namespace BookHive.Frontend.DTOs;

public record UserSummaryResponse(
    int Id,
    string Username,
    string? DisplayName,
    string? AvatarUrl);
