using System.Net;
using System.Net.Http.Json;
using System.Text;
using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

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

    private sealed class BackendFactory(HttpMessageHandler openLibrary)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Jwt:Key", "test-signing-key-test-signing-key-123456");

            builder.ConfigureTestServices(services =>
            {
                services.AddHttpClient<OpenLibraryClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => openLibrary);
            });
        }
    }

    private sealed class StubOpenLibraryHandler(string json) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}