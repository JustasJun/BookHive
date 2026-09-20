using BookHive.Models;

namespace BookHive.Services;

public class LibraryService
{
    private readonly List<LibraryBook> _books = new();
    
    public void AddBook(Book book, ReadingStatus status)
    {
        bool alreadyExists = _books.Any(item =>
            item.Book.Id == book.Id);

        if (alreadyExists)
        {
            return;
        }

        LibraryBook libraryBook = new LibraryBook(book, status);
        _books.Add(libraryBook);
    }

    public List<LibraryBook> GetBooks()
    {
        return _books;
    }

    public void ChangeStatus(string bookId, ReadingStatus newStatus)
    {
        LibraryBook? libraryBook = FindBook(bookId);

        if (libraryBook == null)
        {
            return;
        }

        libraryBook.Status = newStatus;
    }

    public void ToggleFavorite(string bookId)
    {
        LibraryBook? libraryBook = FindBook(bookId);

        if (libraryBook == null)
        {
            return;
        }

        libraryBook.IsFavorite = !libraryBook.IsFavorite;
    }

    public void RemoveBook(string bookId)
    {
        LibraryBook? libraryBook = FindBook(bookId);

        if (libraryBook == null)
        {
            return;
        }

        _books.Remove(libraryBook);
    }

    private LibraryBook? FindBook(string bookId)
    {
        return _books.FirstOrDefault(item =>
            item.Book.Id == bookId);
    }
}