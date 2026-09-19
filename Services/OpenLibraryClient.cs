using System.Text.Json;
using BookHive.Models;

namespace BookHive.Services;

public class OpenLibraryClient
{
    private readonly HttpClient _httpClient; // readonly- reiksme galima prisikirt tik karta (konstruktoriuje)

    public OpenLibraryClient(HttpClient httpClient) //konstruktorius
    {
        _httpClient = httpClient;
    }

    public async Task<List<Book>> SearchAsync(string query, int limit = 10, int page = 1) //async reiškia, kad metodas gali „palaukti" interneto atsakymo neužblokuodamas visos programos.
    {
        string url = $"search.json?q={Uri.EscapeDataString(query)}"
                   + $"&limit={limit}&page={page}"
                   + "&fields=key,title,author_name,first_publish_year,number_of_pages_median,cover_i,subject";

        using Stream stream = await _httpClient.GetStreamAsync(url); //issiuncia uzklausa ir grazina duomenu srauta (stream)

        OpenLibrarySearchResponse? response =
            await JsonSerializer.DeserializeAsync<OpenLibrarySearchResponse>(stream); // skaito JSON is srauto ir deda i musu klase OpenLibrarySearchResponse

        List<Book> books = new();  //sukuriamas tuscias Book sarasas

        if (response == null)
        {
            return books;
        }

        foreach (OpenLibraryDoc doc in response.Docs) // foreach eina per visas gautas knygas po viena. Kiekviena OpenLibraryDoc paverciame Book per ToBook ir pridedame į sarasą.
        {
            books.Add(ToBook(doc));
        }

        return books;
    }

    private static Book ToBook(OpenLibraryDoc doc) // static reiskia, kad jam nereikia klases objekto (nenaudoja _httpClient).
    {
        string author = doc.AuthorNames?.FirstOrDefault() ?? "Nežinomas autorius";

        return new Book(
            Id: doc.Key,
            Title: doc.Title,
            Author: author,
            FirstPublishYear: doc.FirstPublishYear,
            PageCount: doc.PageCount,
            CoverId: doc.CoverId?.ToString(),
            Genre: Genre.Other); // TODO: nustatysim kitame žingsnyje iš doc.Subjects
    }
}