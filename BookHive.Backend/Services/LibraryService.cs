using BookHive.Models;

namespace BookHive.Services;

public class LibraryService
{
    private readonly List<LibraryBook> _books = new();
    
    public void AddBook(int userId, Book book, ReadingStatus status)
    {
        bool alreadyExists = _books.Any(item =>
            item.OpenLibraryBookId == book.Id && item.UserId == userId);

        if (alreadyExists)
        {
            return;
        }

        LibraryBook libraryBook = new LibraryBook(userId, book.Id, status);
        _books.Add(libraryBook);
    }

    public List<LibraryBook> GetBooks(int userId)
    {
        return _books.Where(item => item.UserId == userId).ToList();
    }

    public void ChangeStatus(int userId, string bookId, ReadingStatus newStatus)
    {
        LibraryBook? libraryBook = FindBook(userId, bookId);

        if (libraryBook == null)
        {
            return;
        }

        libraryBook.Status = newStatus;
    }

    public void ToggleFavorite(int userId, string bookId)
    {
        LibraryBook? libraryBook = FindBook(userId, bookId);

        if (libraryBook == null)
        {
            return;
        }

        libraryBook.IsFavorite = !libraryBook.IsFavorite;
    }

    public void RemoveBook(int userId, string bookId)
    {
        LibraryBook? libraryBook = FindBook(userId, bookId);

        if (libraryBook == null)
        {
            return;
        }

        _books.Remove(libraryBook);
    }

    private LibraryBook? FindBook(int userId, string bookId)
    {
        return _books.FirstOrDefault(item =>
            item.OpenLibraryBookId == bookId && item.UserId == userId);
    }
}