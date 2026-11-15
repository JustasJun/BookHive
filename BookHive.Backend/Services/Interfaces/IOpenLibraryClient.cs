using BookHive.Models;

namespace BookHive.Services;

public interface IOpenLibraryClient
{
    Task<List<Book>> SearchAsync(string query, int limit = 10, int page = 1);
    Task<Book?> GetByIdAsync(string id);
}