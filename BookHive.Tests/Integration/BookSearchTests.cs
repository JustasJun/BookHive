using BookHive.Tests.Helpers;
using System.Net;
using System.Net.Http.Json;
using BookHive.Models;

namespace BookHive.Tests.Integration;

public class BookSearchTests
{
    private const string OpenLibraryResponse = """
        {
          "docs": [
            {
              "key": "/works/OL1W",
              "title": "Dune",
              "author_name": ["Frank Herbert"],
              "first_publish_year": 1965,
              "number_of_pages_median": 412,
              "cover_i": 123,
              "subject": ["Science fiction"]
            },
            {
              "key": "/works/OL2W",
              "title": "Emma",
              "author_name": ["Jane Austen"],
              "first_publish_year": 1815,
              "subject": ["Love stories"]
            },
            {
              "key": "/works/OL3W",
              "title": "Neuromancer",
              "author_name": ["William Gibson"],
              "first_publish_year": 1984,
              "subject": ["Dystopian fiction"]
            }
          ]
        }
        """;

    [Fact]
    public async Task Get_WithQueryGenreAndSort_ReturnsMatchingBooksFromOpenLibrary()
    {
        var openLibrary = new StubOpenLibraryHandler(OpenLibraryResponse);
        await using var factory = new BackendFactory(openLibrary);
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(
            "/api/books?query=dune&genres=ScienceFiction&sortBy=title_asc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<Book>? books = await response.Content.ReadFromJsonAsync<List<Book>>();

        Assert.NotNull(books);
        Assert.Equal(
            new[] { "Dune", "Neuromancer" },
            books.Select(book => book.Title));

        Book dune = books[0];
        Assert.Equal("Frank Herbert", dune.Author);
        Assert.Equal(Genre.ScienceFiction, dune.Genre);
        Assert.Equal(1965, dune.FirstPublishYear);
        Assert.Equal("123", dune.CoverId);

        Assert.Contains("q=dune", openLibrary.LastRequestUri?.Query);
    }
}