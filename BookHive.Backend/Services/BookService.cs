using BookHive.Models;

namespace BookHive.Services;

public class BookService : IBookService
{
    private readonly IOpenLibraryClient _client;

    public BookService(IOpenLibraryClient client)
    {
        _client = client;
    }

    public async Task<List<Book>> SearchAsync(string? query, List<Genre>? genres, string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<Book>();
        }

        List<Book> books = await _client.SearchAsync(query);
        books = Filter(books, genres);

        return Sort(books, sortBy);
    }

    public List<Book> Filter(List<Book> books, List<Genre>? genres)
    {
        if (genres == null || genres.Count == 0)
        {
            return books;
        }

        return books.Where(b => genres.Contains(b.Genre)).ToList(); // LINQ
    }

    public List<Book> Sort(List<Book> books, string? sortBy)
    {
        return sortBy switch
        {
            "title_asc" => books.OrderBy(b => b.Title).ToList(),
            "title_desc" => books.OrderByDescending(b => b.Title).ToList(),
            "year_asc" => books.OrderBy(b => b.FirstPublishYear).ToList(),
            "year_desc" => books.OrderByDescending(b => b.FirstPublishYear).ToList(),
            _ => books
        };
    }
}
