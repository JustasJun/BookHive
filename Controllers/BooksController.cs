using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Controllers;

public class BooksController : Controller
{
    private readonly OpenLibraryClient _client;

    public BooksController(OpenLibraryClient client)
    {
        _client = client;
    }

    public async Task<IActionResult> Index(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return View (new List<BookHive.Models.Book>());
            
        }

        var books = await _client.SearchAsync(query);

        return View(books);
    }
}