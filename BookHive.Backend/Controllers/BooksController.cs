using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Backend.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly BookService _bookService;

    public BooksController(BookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Book>>> Get(string? query, [FromQuery] List<Genre>? genres, string? sortBy)
    {
        List<Book> books = await _bookService.SearchAsync(query, genres, sortBy);

        return Ok(books);
    }
}
