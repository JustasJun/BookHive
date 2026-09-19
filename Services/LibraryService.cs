using BookHive.Models;

namespace BookHive.Services;

public class LibraryService
{
    private readonly List<LibraryBook> _books = new();
    
    public void AddBook(Book book, ReadingStatus status)
    {
        bool alreadyExists = _books.Any(libraryBook =>
            libraryBook.Book.Id == book.Id);

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
        LibraryBook? libraryBook = _books.FirstOrDefault(
            libraryBook => libraryBook.Book.Id == bookId);

        if (libraryBook == null)
        {
            return;
        }

        libraryBook.Status = newStatus;
    }

    public void ToggleFavorite(string bookId)
    {
        LibraryBook? libraryBook = _books.FirstOrDefault(
            libraryBook => libraryBook.Book.Id == bookId);

        if (libraryBook == null)
        {
            return;
        }

        libraryBook.IsFavorite = !libraryBook.IsFavorite;
    }

    public void RemoveBook(string bookId)
    {
        LibraryBook? libraryBook = _books.FirstOrDefault(
            libraryBook => libraryBook.Book.Id == bookId);

        if (libraryBook == null)
        {
            return;
        }

        _books.Remove(libraryBook);
    }
}