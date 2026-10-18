namespace BookHive.Models;

public record LibraryBookDetails(
    Book Book,
    ReadingStatus Status,
    bool IsFavorite);
