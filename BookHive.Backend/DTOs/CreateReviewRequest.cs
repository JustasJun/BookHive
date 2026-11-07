namespace BookHive.Backend.DTOs;

public record CreateReviewRequest(
    string OpenLibraryBookId,
    string BookTitle,
    string BookAuthor,
    string? CoverId,
    int Rating,
    string Text);
