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
            "year" => books.OrderByDescending(b => b.FirstPublishYear).ToList(),
            "title" => books.OrderBy(b => b.Title).ToList(),
            _ => books
        };
    }
}