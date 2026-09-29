using BookHive.Backend.Data;
using BookHive.Models;

namespace BookHive.Services;

public class LibraryService
{
    private readonly BookHiveDbContext _context;

    public LibraryService(BookHiveDbContext context)
    {
        _context = context;
    }
    
    public void AddBook(int userId, Book book, ReadingStatus status)
    {
        bool alreadyExists = _context.LibraryBooks.Any(item =>
            item.OpenLibraryBookId == book.Id && item.UserId == userId);

        if (alreadyExists)
        {
            return;
        }

        LibraryBook libraryBook = new LibraryBook(userId, book.Id, status);

        _context.LibraryBooks.Add(libraryBook);
        _context.SaveChanges();
    }

    public List<LibraryBook> GetBooks(int userId)
    {
        return _context.LibraryBooks
            .Where(item => item.UserId == userId)
            .ToList();
    }

    public void ChangeStatus(int userId, string bookId, ReadingStatus newStatus)
    {
        LibraryBook? libraryBook = FindBook(userId, bookId);

        if (libraryBook == null)
        {
            return;
        }

        libraryBook.Status = newStatus;
        _context.SaveChanges();
    }

    public void ToggleFavorite(int userId, string bookId)
    {
        LibraryBook? libraryBook = FindBook(userId, bookId);

        if (libraryBook == null)
        {
            return;
        }

        libraryBook.IsFavorite = !libraryBook.IsFavorite;
        _context.SaveChanges();
    }

    public void RemoveBook(int userId, string bookId)
    {
        LibraryBook? libraryBook = FindBook(userId, bookId);

        if (libraryBook == null)
        {
            return;
        }

        _context.LibraryBooks.Remove(libraryBook);
        _context.SaveChanges();
    }

    private LibraryBook? FindBook(int userId, string bookId)
{
        return _context.LibraryBooks.FirstOrDefault(item =>
            item.OpenLibraryBookId == bookId && item.UserId == userId);
    }
}