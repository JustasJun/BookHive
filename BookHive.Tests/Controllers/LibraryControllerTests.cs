using BookHive.Backend.Controllers;
using BookHive.Backend.Repositories;
using BookHive.Models;
using BookHive.Services;
using BookHive.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using BookHive.Backend.DTOs;
using System.Net;

namespace BookHive.Tests.Controllers;

public class LibraryControllerTests
{
    private const int UserId = 1;

    private readonly ILibraryRepository _libraryRepositorySubstitute =
        Substitute.For<ILibraryRepository>();

    private LibraryController CreateController(int? userId = UserId)
    {
        var openLibraryClient = new OpenLibraryClient(new HttpClient());

        var libraryService = new LibraryService(
            _libraryRepositorySubstitute,
            openLibraryClient);

        return new LibraryController(libraryService).WithUser(userId);
    }

    [Fact]
    public async Task GetIds_WhenUserIsLoggedIn_ReturnsTheirBookIds()
    {
        var bookIds = new List<string>
        {
            "/works/OL123W",
            "/works/OL456W"
        };

        _libraryRepositorySubstitute
            .GetBookIdsAsync(UserId)
            .Returns(bookIds);

        var controller = CreateController();

        ActionResult<List<string>> result = await controller.GetIds();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(bookIds, ok.Value);

        await _libraryRepositorySubstitute
            .Received(1)
            .GetBookIdsAsync(UserId);
    }


    [Fact]
    public async Task GetIds_WhenUserIsNotLoggedIn_ReturnsUnauthorized()
    {
        var controller = CreateController(userId: null);

        ActionResult<List<string>> result = await controller.GetIds();

        Assert.IsType<UnauthorizedResult>(result.Result);

        await _libraryRepositorySubstitute
            .DidNotReceive()
            .GetBookIdsAsync(Arg.Any<int>());
    }


    [Fact]
    public async Task Add_WhenBookDoesNotExist_AddsBookToCurrentUsersLibrary()
    {
        var book = new Book(
            "/works/OL123W",
            "Test Book",
            "Test Author",
            2020,
            250,
            null,
            default);

        var request = new AddBookRequest(
            book,
            ReadingStatus.WantToRead);

        _libraryRepositorySubstitute
            .BookExistsAsync(UserId, book.Id)
            .Returns(false);

        var controller = CreateController();

        IActionResult result = await controller.Add(request);

        Assert.IsType<OkResult>(result);

        _libraryRepositorySubstitute
            .Received(1)
            .Add(Arg.Is<LibraryBook>(b =>
                b.UserId == UserId &&
                b.OpenLibraryBookId == book.Id &&
                b.Status == ReadingStatus.WantToRead));

        await _libraryRepositorySubstitute
            .Received(1)
            .SaveAsync();
    }


    [Fact]
    public async Task Add_WhenBookAlreadyExists_DoesNotAddDuplicate()
    {
        var book = new Book(
            "/works/OL123W",
            "Test Book",
            "Test Author",
            2020,
            250,
            null,
            default);

        var request = new AddBookRequest(
            book,
            ReadingStatus.WantToRead);

        _libraryRepositorySubstitute
            .BookExistsAsync(UserId, book.Id)
            .Returns(true);

        var controller = CreateController();

        IActionResult result = await controller.Add(request);

        Assert.IsType<OkResult>(result);

        await _libraryRepositorySubstitute
            .Received(1)
            .BookExistsAsync(UserId, book.Id);

        _libraryRepositorySubstitute
            .DidNotReceive()
            .Add(Arg.Any<LibraryBook>());

        await _libraryRepositorySubstitute
            .DidNotReceive()
            .SaveAsync();
    }


    [Fact]
    public async Task ChangeStatus_WhenBookExists_UpdatesStatusToRead()
    {
        const string bookId = "/works/OL123W";

        var libraryBook = new LibraryBook(
            UserId,
            bookId,
            ReadingStatus.WantToRead);

        _libraryRepositorySubstitute
            .FindBookAsync(UserId, bookId)
            .Returns(libraryBook);

        var controller = CreateController();

        var request = new ChangeStatusRequest(
            bookId,
            ReadingStatus.Read);

        IActionResult result = await controller.ChangeStatus(request);

        Assert.IsType<OkResult>(result);

        Assert.Equal(ReadingStatus.Read, libraryBook.Status);
        Assert.NotNull(libraryBook.ReadAt);

        await _libraryRepositorySubstitute
            .Received(1)
            .FindBookAsync(UserId, bookId);

        await _libraryRepositorySubstitute
            .Received(1)
            .SaveAsync();
    }


    [Fact]
    public async Task ChangeStatus_WhenBookDoesNotExist_DoesNotSaveChanges()
    {
        const string bookId = "/works/OL999W";

        _libraryRepositorySubstitute
            .FindBookAsync(UserId, bookId)
            .Returns((LibraryBook?)null);

        var controller = CreateController();

        var request = new ChangeStatusRequest(
            bookId,
            ReadingStatus.Read);

        IActionResult result = await controller.ChangeStatus(request);

        Assert.IsType<OkResult>(result);

        await _libraryRepositorySubstitute
            .Received(1)
            .FindBookAsync(UserId, bookId);

        await _libraryRepositorySubstitute
            .DidNotReceive()
            .SaveAsync();
    }


    [Fact]
    public async Task Remove_WhenBookExists_RemovesBookFromCurrentUsersLibrary()
    {
        const string bookId = "/works/OL123W";

        var libraryBook = new LibraryBook(
            UserId,
            bookId,
            ReadingStatus.WantToRead);

        _libraryRepositorySubstitute
            .FindBookAsync(UserId, bookId)
            .Returns(libraryBook);

        var controller = CreateController();

        IActionResult result = await controller.Remove(bookId);

        Assert.IsType<OkResult>(result);

        await _libraryRepositorySubstitute
            .Received(1)
            .FindBookAsync(UserId, bookId);

        _libraryRepositorySubstitute
            .Received(1)
            .Remove(libraryBook);

        await _libraryRepositorySubstitute
            .Received(1)
            .SaveAsync();
    }


    [Fact]
    public async Task Remove_WhenBookDoesNotExist_DoesNotRemoveOrSave()
    {
        const string bookId = "/works/OL999W";

        _libraryRepositorySubstitute
            .FindBookAsync(UserId, bookId)
            .Returns((LibraryBook?)null);

        var controller = CreateController();

        IActionResult result = await controller.Remove(bookId);

        Assert.IsType<OkResult>(result);

        await _libraryRepositorySubstitute
            .Received(1)
            .FindBookAsync(UserId, bookId);

        _libraryRepositorySubstitute
            .DidNotReceive()
            .Remove(Arg.Any<LibraryBook>());

        await _libraryRepositorySubstitute
            .DidNotReceive()
            .SaveAsync();
    }


    [Fact]
    public async Task Get_WhenLibraryIsEmpty_ReturnsEmptyList()
    {
        _libraryRepositorySubstitute
            .GetBooksAsync(UserId)
            .Returns(new List<LibraryBook>());

        var controller = CreateController();

        ActionResult<List<LibraryBookDetails>> result =
            await controller.Get();

        var ok = Assert.IsType<OkObjectResult>(result.Result);

        var books = Assert.IsType<List<LibraryBookDetails>>(ok.Value);
        Assert.Empty(books);

        await _libraryRepositorySubstitute
            .Received(1)
            .GetBooksAsync(UserId);
    }

    [Fact]
    public async Task Get_WhenUserIsNotLoggedIn_ReturnsUnauthorized()
    {
        var controller = CreateController(userId: null);

        var result = await controller.Get();

        Assert.IsType<UnauthorizedResult>(result.Result);

        await _libraryRepositorySubstitute
            .DidNotReceive()
            .GetBooksAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Add_WhenUserIsNotLoggedIn_ReturnsUnauthorized()
    {
        var book = new Book(
            "/works/OL123W",
            "Test Book",
            "Test Author",
            2020,
            250,
            null,
            default);

        var request = new AddBookRequest(
            book,
            ReadingStatus.WantToRead);

        var controller = CreateController(userId: null);

        var result = await controller.Add(request);

        Assert.IsType<UnauthorizedResult>(result);

        await _libraryRepositorySubstitute
            .DidNotReceive()
            .BookExistsAsync(Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ChangeStatus_WhenUserIsNotLoggedIn_ReturnsUnauthorized()
    {
        var controller = CreateController(userId: null);

        var request = new ChangeStatusRequest(
            "/works/OL123W",
            ReadingStatus.Read);

        var result = await controller.ChangeStatus(request);

        Assert.IsType<UnauthorizedResult>(result);

        await _libraryRepositorySubstitute
            .DidNotReceive()
            .FindBookAsync(Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Remove_WhenUserIsNotLoggedIn_ReturnsUnauthorized()
    {
        var controller = CreateController(userId: null);

        var result = await controller.Remove("/works/OL123W");

        Assert.IsType<UnauthorizedResult>(result);

        await _libraryRepositorySubstitute
            .DidNotReceive()
            .FindBookAsync(Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ChangeStatus_WhenChangingFromRead_ClearsReadDate()
    {
        const string bookId = "/works/OL123W";

        var libraryBook = new LibraryBook(
            UserId,
            bookId,
            ReadingStatus.Read)
        {
            ReadAt = DateTime.UtcNow.AddDays(-5)
        };

        _libraryRepositorySubstitute
            .FindBookAsync(UserId, bookId)
            .Returns(libraryBook);

        var controller = CreateController();

        var request = new ChangeStatusRequest(
            bookId,
            ReadingStatus.WantToRead);

        var result = await controller.ChangeStatus(request);

        Assert.IsType<OkResult>(result);
        Assert.Equal(ReadingStatus.WantToRead, libraryBook.Status);
        Assert.Null(libraryBook.ReadAt);

        await _libraryRepositorySubstitute
            .Received(1)
            .SaveAsync();
    }

    [Fact]
    public async Task ChangeStatus_WhenAlreadyRead_PreservesReadDate()
    {
        const string bookId = "/works/OL123W";

        var originalReadDate = DateTime.UtcNow.AddDays(-5);

        var libraryBook = new LibraryBook(
            UserId,
            bookId,
            ReadingStatus.Read)
        {
            ReadAt = originalReadDate
        };

        _libraryRepositorySubstitute
            .FindBookAsync(UserId, bookId)
            .Returns(libraryBook);

        var controller = CreateController();

        var request = new ChangeStatusRequest(
            bookId,
            ReadingStatus.Read);

        var result = await controller.ChangeStatus(request);

        Assert.IsType<OkResult>(result);
        Assert.Equal(ReadingStatus.Read, libraryBook.Status);
        Assert.Equal(originalReadDate, libraryBook.ReadAt);

        await _libraryRepositorySubstitute
            .Received(1)
            .SaveAsync();
    }

    [Fact]
    public async Task Get_WhenLibraryContainsBook_ReturnsBookDetails()
    {
        const string bookId = "/works/OL123W";

        var libraryBook = new LibraryBook(
            UserId,
            bookId,
            ReadingStatus.Read)
        {
            IsFavorite = true
        };

        _libraryRepositorySubstitute
            .GetBooksAsync(UserId)
            .Returns(new List<LibraryBook> { libraryBook });

        var handler = new StubHttpMessageHandler(_ =>
            StubHttpMessageHandler.Json("""
            {
                "docs": [
                    {
                        "key": "/works/OL123W",
                        "title": "Test Book",
                        "author_name": ["Test Author"],
                        "first_publish_year": 2020,
                        "number_of_pages_median": 250,
                        "subject": []
                    }
                ]
            }
            """));

        var service = new LibraryService(
            _libraryRepositorySubstitute,
            handler.CreateClient());

        var controller = new LibraryController(service).WithUser(UserId);

        var result = await controller.Get();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var books = Assert.IsType<List<LibraryBookDetails>>(ok.Value);

        var item = Assert.Single(books);

        Assert.Equal(bookId, item.Book.Id);
        Assert.Equal("Test Book", item.Book.Title);
        Assert.Equal(ReadingStatus.Read, item.Status);
        Assert.True(item.IsFavorite);

        await _libraryRepositorySubstitute
            .Received(1)
            .GetBooksAsync(UserId);

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Get_WhenOpenLibraryRequestFails_ReturnsEmptyList()
    {
        const string bookId = "/works/OL123W";

        var libraryBook = new LibraryBook(
            UserId,
            bookId,
            ReadingStatus.WantToRead);

        _libraryRepositorySubstitute
            .GetBooksAsync(UserId)
            .Returns(new List<LibraryBook> { libraryBook });

        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        var service = new LibraryService(
            _libraryRepositorySubstitute,
            handler.CreateClient());

        var controller = new LibraryController(service).WithUser(UserId);

        var result = await controller.Get();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var books = Assert.IsType<List<LibraryBookDetails>>(ok.Value);

        Assert.Empty(books);

        await _libraryRepositorySubstitute
            .Received(1)
            .GetBooksAsync(UserId);

        Assert.Single(handler.Requests);
    }
}