using System.Net;
using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Services;
using BookHive.Tests.Helpers;

namespace BookHive.Tests.Services;

public class LibraryServiceTests
{
    private const int UserId = 1;
    private const string BookId = "/works/OL1W";
    private const string EmptyDocs = """{"docs":[]}""";

    private readonly ILibraryRepository _repository =
        Substitute.For<ILibraryRepository>();

    [Fact]
    public async Task AddBookAsync_WhenBookAlreadyInLibrary_DoesNotAddOrSave()
    {
        _repository.BookExistsAsync(UserId, BookId).Returns(true);
        LibraryService service = CreateService();

        await service.AddBookAsync(UserId, CreateBook(), ReadingStatus.WantToRead);

        _repository.DidNotReceive().Add(Arg.Any<LibraryBook>());
        await _repository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task AddBookAsync_WhenBookIsNew_AddsWithGivenStatusAndSaves()
    {
        _repository.BookExistsAsync(UserId, BookId).Returns(false);
        LibraryService service = CreateService();

        await service.AddBookAsync(UserId, CreateBook(), ReadingStatus.CurrentlyReading);

        _repository.Received(1).Add(Arg.Is<LibraryBook>(item =>
            item.UserId == UserId &&
            item.OpenLibraryBookId == BookId &&
            item.Status == ReadingStatus.CurrentlyReading &&
            item.ReadAt == null));
        await _repository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task AddBookAsync_WhenStatusIsRead_SetsReadAt()
    {
        LibraryService service = CreateService();

        await service.AddBookAsync(UserId, CreateBook(), ReadingStatus.Read);

        _repository.Received(1).Add(Arg.Is<LibraryBook>(item =>
            item.Status == ReadingStatus.Read && item.ReadAt != null));
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenBookIsMissing_DoesNotSave()
    {
        LibraryService service = CreateService();

        await service.ChangeStatusAsync(UserId, BookId, ReadingStatus.Read);

        await _repository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenMarkedAsRead_SetsReadAtAndSaves()
    {
        var libraryBook = new LibraryBook(UserId, BookId, ReadingStatus.WantToRead);
        _repository.FindBookAsync(UserId, BookId).Returns(libraryBook);
        LibraryService service = CreateService();
        DateTime before = DateTime.UtcNow;

        await service.ChangeStatusAsync(UserId, BookId, ReadingStatus.Read);

        Assert.Equal(ReadingStatus.Read, libraryBook.Status);
        Assert.NotNull(libraryBook.ReadAt);
        Assert.InRange(libraryBook.ReadAt.Value, before, DateTime.UtcNow);
        await _repository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenAlreadyRead_KeepsOriginalReadAt()
    {
        var originalReadAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var libraryBook = new LibraryBook(UserId, BookId, ReadingStatus.Read)
        {
            ReadAt = originalReadAt
        };
        _repository.FindBookAsync(UserId, BookId).Returns(libraryBook);
        LibraryService service = CreateService();

        await service.ChangeStatusAsync(UserId, BookId, ReadingStatus.Read);

        Assert.Equal(originalReadAt, libraryBook.ReadAt);
    }

    [Theory]
    [InlineData(ReadingStatus.WantToRead)]
    [InlineData(ReadingStatus.CurrentlyReading)]
    public async Task ChangeStatusAsync_WhenMovedAwayFromRead_ClearsReadAt(
        ReadingStatus newStatus)
    {
        var libraryBook = new LibraryBook(UserId, BookId, ReadingStatus.Read);
        _repository.FindBookAsync(UserId, BookId).Returns(libraryBook);
        LibraryService service = CreateService();

        await service.ChangeStatusAsync(UserId, BookId, newStatus);

        Assert.Equal(newStatus, libraryBook.Status);
        Assert.Null(libraryBook.ReadAt);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task ToggleFavoriteAsync_FlipsFavoriteFlagAndSaves(
        bool before, bool after)
    {
        var libraryBook = new LibraryBook(UserId, BookId, ReadingStatus.Read)
        {
            IsFavorite = before
        };
        _repository.FindBookAsync(UserId, BookId).Returns(libraryBook);
        LibraryService service = CreateService();

        await service.ToggleFavoriteAsync(UserId, BookId);

        Assert.Equal(after, libraryBook.IsFavorite);
        await _repository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task ToggleFavoriteAsync_WhenBookIsMissing_DoesNotSave()
    {
        LibraryService service = CreateService();

        await service.ToggleFavoriteAsync(UserId, BookId);

        await _repository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task RemoveBookAsync_WhenBookExists_RemovesAndSaves()
    {
        var libraryBook = new LibraryBook(UserId, BookId, ReadingStatus.Read);
        _repository.FindBookAsync(UserId, BookId).Returns(libraryBook);
        LibraryService service = CreateService();

        await service.RemoveBookAsync(UserId, BookId);

        _repository.Received(1).Remove(libraryBook);
        await _repository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task RemoveBookAsync_WhenBookIsMissing_DoesNothing()
    {
        LibraryService service = CreateService();

        await service.RemoveBookAsync(UserId, BookId);

        _repository.DidNotReceive().Remove(Arg.Any<LibraryBook>());
        await _repository.DidNotReceive().SaveAsync();
    }

    [Fact]
    public async Task GetBooksAsync_ReturnsBooksFromRepository()
    {
        var books = new List<LibraryBook>
        {
            new(UserId, BookId, ReadingStatus.Read)
        };
        _repository.GetBooksAsync(UserId).Returns(books);
        LibraryService service = CreateService();

        List<LibraryBook> result = await service.GetBooksAsync(UserId);

        Assert.Same(books, result);
    }

    [Fact]
    public async Task GetBookIdsAsync_ReturnsIdsFromRepository()
    {
        _repository.GetBookIdsAsync(UserId)
            .Returns(new List<string> { BookId, "/works/OL2W" });
        LibraryService service = CreateService();

        List<string> ids = await service.GetBookIdsAsync(UserId);

        Assert.Equal(new[] { BookId, "/works/OL2W" }, ids);
    }

    [Fact]
    public async Task GetBookDetailsAsync_CombinesLibraryEntriesWithOpenLibraryData()
    {
        _repository.GetBooksAsync(UserId).Returns(new List<LibraryBook>
        {
            new(UserId, "/works/OL1W", ReadingStatus.Read) { IsFavorite = true }
        });
        LibraryService service = CreateService(request =>
            StubHttpMessageHandler.Json(DocJson("/works/OL1W", "Dune")));

        List<LibraryBookDetails> details = await service.GetBookDetailsAsync(UserId);

        LibraryBookDetails item = Assert.Single(details);
        Assert.Equal("Dune", item.Book.Title);
        Assert.Equal(ReadingStatus.Read, item.Status);
        Assert.True(item.IsFavorite);
    }

    [Fact]
    public async Task GetBookDetailsAsync_WhenOpenLibraryFailsForOneBook_SkipsOnlyThatBook()
    {
        _repository.GetBooksAsync(UserId).Returns(new List<LibraryBook>
        {
            new(UserId, "/works/OL1W", ReadingStatus.Read),
            new(UserId, "/works/OL2W", ReadingStatus.WantToRead)
        });
        LibraryService service = CreateService(request =>
            request.RequestUri!.Query.Contains("OL2W")
                ? StubHttpMessageHandler.Json(EmptyDocs, HttpStatusCode.InternalServerError)
                : StubHttpMessageHandler.Json(DocJson("/works/OL1W", "Dune")));

        List<LibraryBookDetails> details = await service.GetBookDetailsAsync(UserId);

        Assert.Equal("Dune", Assert.Single(details).Book.Title);
    }

    [Fact]
    public async Task GetBookDetailsAsync_WhenBookIsUnknownToOpenLibrary_SkipsIt()
    {
        _repository.GetBooksAsync(UserId).Returns(new List<LibraryBook>
        {
            new(UserId, BookId, ReadingStatus.Read)
        });
        LibraryService service = CreateService();

        List<LibraryBookDetails> details = await service.GetBookDetailsAsync(UserId);

        Assert.Empty(details);
    }

    private LibraryService CreateService(
        Func<HttpRequestMessage, HttpResponseMessage>? respond = null)
    {
        var handler = new StubHttpMessageHandler(
            respond ?? (_ => StubHttpMessageHandler.Json(EmptyDocs)));

        return new LibraryService(_repository, handler.CreateClient());
    }

    private static Book CreateBook() =>
        new(BookId, "Dune", "Frank Herbert", 1965, 412, null, Genre.ScienceFiction);

    private static string DocJson(string key, string title) =>
        $$"""{"docs":[{"key":"{{key}}","title":"{{title}}","author_name":["Author"],"subject":["Fantasy"]}]}""";
}