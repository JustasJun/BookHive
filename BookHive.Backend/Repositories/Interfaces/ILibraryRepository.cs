using BookHive.Models;

namespace BookHive.Backend.Repositories;

public interface ILibraryRepository : IRepository<LibraryBook>
{
    Task<bool> BookExistsAsync(int userId, string bookId);
    Task<LibraryBook?> FindBookAsync(int userId, string bookId);
    Task<List<LibraryBook>> GetBooksAsync(int userId);
    Task<List<string>> GetBookIdsAsync(int userId);
    Task<int> CountReadBooksAsync(int userId, DateTime from, DateTime to);
}