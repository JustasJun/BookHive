using BookHive.Models;

namespace BookHive.Services;

public interface ILibraryService
{
    Task AddBookAsync(int userId, Book book, ReadingStatus status);
    Task<List<LibraryBook>> GetBooksAsync(int userId);
    Task<List<LibraryBookDetails>> GetBookDetailsAsync(int userId);
    Task<List<string>> GetBookIdsAsync(int userId);
    Task ChangeStatusAsync(int userId, string bookId, ReadingStatus newStatus);
    Task ToggleFavoriteAsync(int userId, string bookId);
    Task RemoveBookAsync(int userId, string bookId);
}