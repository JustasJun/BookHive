using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Backend.Controllers;

[ApiController]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    private readonly LibraryService _libraryService;

    public LibraryController(LibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<List<LibraryBookDetails>>> Get(int userId)
    {
        var books = await _libraryService.GetBookDetailsAsync(userId);
        return Ok(books);
    }

    [HttpPost]
    public IActionResult Add([FromBody] AddBookRequest request)
    {
        _libraryService.AddBook(
            request.UserId,
            request.Book,
            request.Status);

        return Ok();
    }

    [HttpPut("status")]
    public IActionResult ChangeStatus([FromBody] ChangeStatusRequest request)
    {
        _libraryService.ChangeStatus(
            request.UserId,
            request.BookId,
            request.Status);

        return Ok();
    }

    public record ChangeStatusRequest(
        int UserId,
        string BookId,
        ReadingStatus Status);

    public record AddBookRequest(
        int UserId,
        Book Book,
        ReadingStatus Status);

    [HttpDelete]
    public IActionResult Remove([FromQuery] int userId, [FromQuery] string bookId)
    {
        _libraryService.RemoveBook(userId, bookId);
        return Ok();
    }

}