using BookHive.Models;
using BookHive.Services;
using BookHive.Tests.Helpers;

namespace BookHive.Tests.Services;

public class BookServiceTests
{
    private const string SearchResponse = """
        {
          "docs": [
            { "key": "/works/OL1W", "title": "Dune", "author_name": ["Frank Herbert"], "subject": ["Science fiction"] },
            { "key": "/works/OL2W", "title": "Emma", "author_name": ["Jane Austen"], "subject": ["Love stories"] },
            { "key": "/works/OL3W", "title": "Neuromancer", "author_name": ["William Gibson"], "subject": ["Dystopian fiction"] }
          ]
        }
        """;

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchAsync_WhenQueryIsBlank_ReturnsEmptyWithoutCallingOpenLibrary(
        string? query)
    {
        var handler = new StubHttpMessageHandler(_ =>
            StubHttpMessageHandler.Json(SearchResponse));
        var service = new BookService(handler.CreateClient());

        List<Book> books = await service.SearchAsync(query, null, null);

        Assert.Empty(books);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task SearchAsync_FiltersByGenreAndSorts()
    {
        var handler = new StubHttpMessageHandler(_ =>
            StubHttpMessageHandler.Json(SearchResponse));
        var service = new BookService(handler.CreateClient());

        List<Book> books = await service.SearchAsync(
            "sci-fi", new List<Genre> { Genre.ScienceFiction }, "title_desc");

        Assert.Equal(new[] { "Neuromancer", "Dune" }, books.Select(book => book.Title));
        Assert.Single(handler.Requests);
    }

    [Fact]
    public void Filter_WhenGenresAreNullOrEmpty_ReturnsAllBooks()
    {
        BookService service = CreateService();
        List<Book> books = CreateBooks();

        Assert.Same(books, service.Filter(books, null));
        Assert.Same(books, service.Filter(books, new List<Genre>()));
    }

    [Fact]
    public void Filter_KeepsOnlyBooksWithSelectedGenres()
    {
        BookService service = CreateService();
        var books = new List<Book>
        {
            CreateBook("1", "A", 2000, Genre.Fantasy),
            CreateBook("2", "B", 2001, Genre.Horror),
            CreateBook("3", "C", 2002, Genre.Romance)
        };

        List<Book> result = service.Filter(
            books, new List<Genre> { Genre.Fantasy, Genre.Romance });

        Assert.Equal(new[] { "A", "C" }, result.Select(book => book.Title));
    }

    [Theory]
    [InlineData("title_asc", "A,B,C")]
    [InlineData("title_desc", "C,B,A")]
    [InlineData("year_asc", "C,B,A")]
    [InlineData("year_desc", "A,B,C")]
    [InlineData(null, "B,A,C")]
    [InlineData("unknown", "B,A,C")]
    public void Sort_OrdersBooksAccordingToSortKey(string? sortBy, string expectedTitles)
    {
        BookService service = CreateService();
        var books = new List<Book>
        {
            CreateBook("1", "B", 2000, Genre.Other),
            CreateBook("2", "A", 2010, Genre.Other),
            CreateBook("3", "C", 1990, Genre.Other)
        };

        List<Book> result = service.Sort(books, sortBy);

        Assert.Equal(expectedTitles, string.Join(",", result.Select(book => book.Title)));
    }

    private static BookService CreateService()
    {
        var handler = new StubHttpMessageHandler(_ =>
            StubHttpMessageHandler.Json("""{"docs":[]}"""));

        return new BookService(handler.CreateClient());
    }

    private static List<Book> CreateBooks() => new()
    {
        CreateBook("1", "A", 2000, Genre.Fantasy)
    };

    private static Book CreateBook(string id, string title, int year, Genre genre) =>
        new(id, title, "Author", year, null, null, genre);
}