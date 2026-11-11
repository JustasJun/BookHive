using BookHive.Backend.Data;
using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Tests.Helpers;

namespace BookHive.Tests.Repositories;

public class LibraryRepositoryTests : IDisposable
{
    private static readonly DateTime From =
        new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime To =
        new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly BookHiveDbContext _context = TestDbContextFactory.Create();
    private readonly LibraryRepository _repository;

    public LibraryRepositoryTests()
    {
        _repository = new LibraryRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private LibraryBook AddBook(
        int userId,
        string bookId,
        ReadingStatus status = ReadingStatus.WantToRead,
        DateTime? readAt = null)
    {
        var book = new LibraryBook(userId, bookId, status)
        {
            ReadAt = readAt
        };
        _context.LibraryBooks.Add(book);
        _context.SaveChanges();

        return book;
    }

    [Fact]
    public async Task BookExistsAsync_WhenUserHasBook_ReturnsTrue()
    {
        AddBook(1, "OL1W");

        Assert.True(await _repository.BookExistsAsync(1, "OL1W"));
    }

    [Fact]
    public async Task BookExistsAsync_WhenOnlyOtherUserHasBook_ReturnsFalse()
    {
        AddBook(2, "OL1W");

        Assert.False(await _repository.BookExistsAsync(1, "OL1W"));
    }

    [Fact]
    public async Task FindBookAsync_WhenBookExists_ReturnsIt()
    {
        LibraryBook book = AddBook(1, "OL1W");

        LibraryBook? result = await _repository.FindBookAsync(1, "OL1W");

        Assert.Same(book, result);
    }

    [Fact]
    public async Task FindBookAsync_WhenBookDoesNotExist_ReturnsNull()
    {
        AddBook(1, "OL1W");

        LibraryBook? result = await _repository.FindBookAsync(1, "OL999W");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBooksAsync_ReturnsOnlyThatUsersBooksInInsertOrder()
    {
        AddBook(1, "OL1W");
        AddBook(2, "OL2W");
        AddBook(1, "OL3W");

        List<LibraryBook> result = await _repository.GetBooksAsync(1);

        Assert.Equal(
            new[] { "OL1W", "OL3W" },
            result.Select(book => book.OpenLibraryBookId));
    }

    [Fact]
    public async Task GetBookIdsAsync_ReturnsOnlyThatUsersBookIds()
    {
        AddBook(1, "OL1W");
        AddBook(2, "OL2W");

        List<string> result = await _repository.GetBookIdsAsync(1);

        Assert.Equal(new[] { "OL1W" }, result);
    }

    [Fact]
    public async Task AddAndSaveAsync_PersistsBook()
    {
        var book = new LibraryBook(1, "OL1W", ReadingStatus.WantToRead);

        _repository.Add(book);
        await _repository.SaveAsync();

        Assert.Single(_context.LibraryBooks);
    }

    [Fact]
    public async Task RemoveAndSaveAsync_DeletesBook()
    {
        LibraryBook book = AddBook(1, "OL1W");

        _repository.Remove(book);
        await _repository.SaveAsync();

        Assert.Empty(_context.LibraryBooks);
    }

    [Fact]
    public async Task CountReadBooksAsync_CountsOnlyReadBooksInsideRange()
    {
        AddBook(1, "OL1W", ReadingStatus.Read, From.AddDays(5));
        AddBook(1, "OL2W", ReadingStatus.Read, From.AddDays(20));
        AddBook(1, "OL3W", ReadingStatus.Read, From.AddDays(-1));
        AddBook(1, "OL4W", ReadingStatus.Read, To.AddDays(1));
        AddBook(1, "OL5W", ReadingStatus.WantToRead, From.AddDays(5));
        AddBook(2, "OL6W", ReadingStatus.Read, From.AddDays(5));

        int count = await _repository.CountReadBooksAsync(1, From, To);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CountReadBooksAsync_RangeStartIsInclusiveAndEndIsExclusive()
    {
        AddBook(1, "OL1W", ReadingStatus.Read, From);
        AddBook(1, "OL2W", ReadingStatus.Read, To);

        int count = await _repository.CountReadBooksAsync(1, From, To);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CountReadBooksAsync_IgnoresReadBooksWithoutReadDate()
    {
        AddBook(1, "OL1W", ReadingStatus.Read, readAt: null);

        int count = await _repository.CountReadBooksAsync(1, From, To);

        Assert.Equal(0, count);
    }
}