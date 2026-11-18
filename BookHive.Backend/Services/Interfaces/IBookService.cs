using BookHive.Models;

namespace BookHive.Services;

public interface IBookService
{
    Task<List<Book>> SearchAsync(string? query, List<Genre>? genres, string? sortBy);
}
