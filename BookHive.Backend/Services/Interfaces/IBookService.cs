using BookHive.Models;

namespace BookHive.Services;

public interface IBookService
{
    Task<List<Book>> SearchAsync(String? query, List<Genre>? genres, string? sorbBy);
}