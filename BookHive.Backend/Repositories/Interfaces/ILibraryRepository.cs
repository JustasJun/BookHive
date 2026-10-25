using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface ILibraryRepository
{
    Task<bool> BookExistsAsync(int userId, string bookId);
    Task<LibraryBook?> FindBookAsync(int userId, string bookId);
    Task<List<LibraryBook>> GetBooksAsync(int userId);
    Task<List<string>> GetBookIdsAsync(int userId);
    void Add(LibraryBook libraryBook);
    void Remove(LibraryBook libraryBook);
    Task<int> CountReadBooksAsync(int userId, DateTime from, DateTime to);
    Task SaveAsync();
}