using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Backend.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly OpenLibraryClient _client;
    private readonly BookService _bookService;

    public BooksController(OpenLibraryClient client, BookService bookService)
    {
        _client = client;
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Book>>> Get(string? query, Genre? genre, string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Ok(new List<Book>());
        }

        var books = await _client.SearchAsync(query);
        books = _bookService.Filter(books, genre);
        books = _bookService.Sort(books, sortBy);

        return Ok(books);
    }
}