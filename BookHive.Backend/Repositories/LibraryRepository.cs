using BookHive.Backend.Data;
using BookHive.Models;

namespace BookHive.Backend.Repositories;

public class LibraryRepository : ILibraryRepository
{
    private readonly BookHiveDbContext _context;

    public LibraryRepository(BookHiveDbContext context)
    {
        _context = context;
    }

    public bool BookExists(int userId, string bookId)
    {
        return _context.LibraryBooks.Any(item =>
            item.UserId == userId &&
            item.OpenLibraryBookId == bookId);
    }

    public LibraryBook? FindBook(int userId, string bookId)
    {
        return _context.LibraryBooks.FirstOrDefault(item =>
            item.UserId == userId &&
            item.OpenLibraryBookId == bookId);
    }

    public List<LibraryBook> GetBooks(int userId)
    {
        return _context.LibraryBooks
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.Id) 
            .ToList();
    }

    public List<string> GetBookIds(int userId)
{
    return _context.LibraryBooks
        .Where(item => item.UserId == userId)
        .Select(item => item.OpenLibraryBookId)
        .ToList();
}

    public void Add(LibraryBook libraryBook)
    {
        _context.LibraryBooks.Add(libraryBook);
    }

    public void Remove(LibraryBook libraryBook)
    {
        _context.LibraryBooks.Remove(libraryBook);
    }

    public int CountReadBooks(int userId, DateTime from, DateTime to)
    {
        return _context.LibraryBooks.Count(item =>
            item.UserId == userId &&
            item.Status == ReadingStatus.Read &&
            item.ReadAt != null &&
            item.ReadAt >= from &&
            item.ReadAt < to);
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}