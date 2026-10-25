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

    public async Task AddBookAsync(int userId, Book book, ReadingStatus status)
    {
        bool alreadyExists =
            await _libraryRepository.BookExistsAsync(userId, book.Id);

        if (alreadyExists)
        {
            return;
        }

        LibraryBook libraryBook = new LibraryBook(userId, book.Id, status);

        _libraryRepository.Add(libraryBook);
        await _libraryRepository.SaveAsync();
    }

    public async Task<List<LibraryBook>> GetBooksAsync(int userId)
    {
        return await _libraryRepository.GetBooksAsync(userId);
    }

    public async Task<List<LibraryBookDetails>> GetBookDetailsAsync(int userId)
    {
        var libraryBooks = await _libraryRepository.GetBooksAsync(userId);

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

    public async Task<List<string>> GetBookIdsAsync(int userId)
    {
        return await _libraryRepository.GetBookIdsAsync(userId);
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

    public async Task ChangeStatusAsync(
        int userId, string bookId, ReadingStatus newStatus)
    {
        LibraryBook? libraryBook =
            await _libraryRepository.FindBookAsync(userId, bookId);

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
        await _libraryRepository.SaveAsync();
    }

    public async Task ToggleFavoriteAsync(int userId, string bookId)
    {
        LibraryBook? libraryBook =
            await _libraryRepository.FindBookAsync(userId, bookId);

        if (libraryBook == null)
        {
            return;
        }

        libraryBook.IsFavorite = !libraryBook.IsFavorite;
        await _libraryRepository.SaveAsync();
    }

    public async Task RemoveBookAsync(int userId, string bookId)
    {
        LibraryBook? libraryBook =
            await _libraryRepository.FindBookAsync(userId, bookId);

        if (libraryBook == null)
        {
            return;
        }

        _libraryRepository.Remove(libraryBook);
        await _libraryRepository.SaveAsync();
    }
}