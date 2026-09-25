using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Controllers;

public class BooksController : Controller
{
    private readonly OpenLibraryClient _client;
    private readonly BookService _bookService;

    public BooksController(OpenLibraryClient client, BookService bookService)
    {
        _client = client;
        _bookService = bookService;
    }

    public async Task<IActionResult> Index(string? query, Genre? genre, string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return View (new List<Book>());
            
        }

        var books = await _client.SearchAsync(query);
        books = _bookService.Filter(books, genre); 
        books = _bookService.Sort(books, sortBy);

        return View(books);
    }
}