namespace BookHive.Models;

public record CommunityReviewItem(
    int ReviewId,
    int UserId,
    string Username,
    string BookTitle,
    string BookAuthor,
    string? CoverId,
    int Rating,
    string Text,
    DateTime CreatedAt);