using System.Text.Json;
using BookHive.Models;
using BookHive.Extensions;

namespace BookHive.Services;

public class OpenLibraryClient
{
    private readonly HttpClient _httpClient; 
    public OpenLibraryClient(HttpClient httpClient) 
    {
        _httpClient = httpClient;
    }

    public async Task<List<Book>> SearchAsync(string query, int limit = 10, int page = 1)
    {
        string url = $"search.json?q={Uri.EscapeDataString(query)}"
                   + $"&limit={limit}&page={page}"
                   + "&fields=key,title,author_name,first_publish_year,number_of_pages_median,cover_i,subject";

        // Read the response as a stream so the JSON is deserialized while it arrives
        using Stream stream = await _httpClient.GetStreamAsync(url);

         // Read JSON from stream and put to our class OpenLibrarySearchResponse
        OpenLibrarySearchResponse? response =
            await JsonSerializer.DeserializeAsync<OpenLibrarySearchResponse>(stream);

        List<Book> books = new();

        if (response == null)
        {
            return books;
        }

        foreach (OpenLibraryDoc doc in response.Docs)
        {
            books.Add(ToBook(doc));
        }

        return books;
    }

    // Static means that this method does not require an instance of the class (does not use _httpClient).
    private static Book ToBook(OpenLibraryDoc doc)
    {
        string author = doc.AuthorNames?.FirstOrDefault() ?? "Unknown author";

        return new Book(
            Id: doc.Key,
            Title: doc.Title,
            Author: author,
            FirstPublishYear: doc.FirstPublishYear,
            PageCount: doc.PageCount,
            CoverId: doc.CoverId?.ToString(),
            Genre: doc.Subjects.ToGenre());
    }

    public async Task<Book?> GetByIdAsync(string id)
    {
        string url = $"search.json?q=key:{Uri.EscapeDataString(id)}"
                + "&fields=key,title,author_name,first_publish_year,number_of_pages_median,cover_i,subject";

        using Stream stream = await _httpClient.GetStreamAsync(url);

        OpenLibrarySearchResponse? response =
            await JsonSerializer.DeserializeAsync<OpenLibrarySearchResponse>(stream);

        OpenLibraryDoc? doc = response?.Docs.FirstOrDefault();

        return doc == null ? null : ToBook(doc);
    }
}