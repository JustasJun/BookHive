using BookHive.Models;

namespace BookHive.Services;

public class BookService
{
    public List<Book> Filter(List<Book> books, Genre? genre)
    {
        if (genre == null)
        {
            return books;
        }

        return books.Where(b => b.Genre == genre).ToList();
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