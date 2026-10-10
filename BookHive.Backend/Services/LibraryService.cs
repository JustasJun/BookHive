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
        var result = new List<LibraryBookDetails>();

        foreach (var item in libraryBooks)
        {
            Book? book = await _client.GetByIdAsync(item.OpenLibraryBookId);

            if (book != null)
            {
                result.Add(new LibraryBookDetails(
                    book,
                    item.Status,
                    item.IsFavorite));
            }
        }

        return result;
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
        DateTime yearStart = new DateTime(goal.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime yearEnd = yearStart.AddYears(1);
        DateTime from = goal.StartedAt > yearStart ? goal.StartedAt : yearStart;

        return _libraryRepository.CountReadBooks(userId, from, yearEnd);
    }
}