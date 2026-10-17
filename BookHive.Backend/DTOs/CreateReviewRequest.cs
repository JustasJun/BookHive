namespace BookHive.Backend.DTOs;

public record CreateReviewRequest(
    int UserId,
    string OpenLibraryBookId,
    string BookTitle,
    string BookAuthor,
    string? CoverId,
    int Rating,
    string Text);