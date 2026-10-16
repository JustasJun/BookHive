using BookHive.Backend.Repositories;
using BookHive.Models;

namespace BookHive.Services;

public class LibraryService
{
    private readonly ILibraryRepository _libraryRepository;
    private readonly OpenLibraryClient _client;

    public LibraryService(
        ILibraryRepository libraryRepository,
        OpenLibraryClient client)
    {
        _libraryRepository = libraryRepository;
        _client = client;
    }

    public void AddBook(int userId, Book book, ReadingStatus status)
    {
        bool alreadyExists = _libraryRepository.BookExists(userId, book.Id);

        if (alreadyExists)
        {
            return;
        }

        LibraryBook libraryBook = new LibraryBook(userId, book.Id, status);

        _libraryRepository.Add(libraryBook);
        _libraryRepository.Save();
    }

    public List<LibraryBook> GetBooks(int userId)
    {
        return _libraryRepository.GetBooks(userId);
    }

    public async Task<List<LibraryBookDetails>> GetBookDetailsAsync(int userId)
    {
        var libraryBooks = _libraryRepository.GetBooks(userId);
        
        var lookups = libraryBooks.Select(async item =>
        {
            Book? book = await TryGetBookAsync(item.OpenLibraryBookId);

            return book == null
                ? null
                : new LibraryBookDetails(book, item.Status, item.IsFavorite);
        });

        var details = await Task.WhenAll(lookups);

        return details.OfType<LibraryBookDetails>().ToList();
    }

    public List<string> GetBookIds(int userId)
    {
        return _libraryRepository.GetBookIds(userId);
    }

    private async Task<Book?> TryGetBookAsync(string bookId)
    {
        try
        {
            return await _client.GetByIdAsync(bookId);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public void ChangeStatus(int userId, string bookId, ReadingStatus newStatus)
    {
        LibraryBook? libraryBook = _libraryRepository.FindBook(userId, bookId);

        if (libraryBook == null)
        {
            return;
        }

        if (newStatus == ReadingStatus.Read &&
            libraryBook.Status != ReadingStatus.Read)
        {
            libraryBook.ReadAt = DateTime.UtcNow;
        }
        else if (newStatus != ReadingStatus.Read)
        {
            libraryBook.ReadAt = null;
        }

        libraryBook.Status = newStatus;
        _libraryRepository.Save();
    }

    public void ToggleFavorite(int userId, string bookId)
    {
        LibraryBook? libraryBook = _libraryRepository.FindBook(userId, bookId);

        if (libraryBook == null)
        {
            return;
        }

        libraryBook.IsFavorite = !libraryBook.IsFavorite;
        _libraryRepository.Save();
    }

    public void RemoveBook(int userId, string bookId)
    {
        LibraryBook? libraryBook = _libraryRepository.FindBook(userId, bookId);

        if (libraryBook == null)
        {
            return;
        }

        _libraryRepository.Remove(libraryBook);
        _libraryRepository.Save();
    }

    public int CountReadForGoal(int userId, ReadingGoal goal)
    {
        return _libraryRepository.CountReadBooks(userId, goal.StartedAt, goal.EndsAt);
    }
}