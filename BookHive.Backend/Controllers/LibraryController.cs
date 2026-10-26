using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;
using BookHive.Backend.DTOs;

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

    [HttpGet("{userId}/ids")]
    public async Task<ActionResult<List<string>>> GetIds(int userId)
    {
        var ids = await _libraryService.GetBookIdsAsync(userId);
        return Ok(ids);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddBookRequest request)
    {
        await _libraryService.AddBookAsync(
            request.UserId,
            request.Book,
            request.Status);

        return Ok();
    }

    [HttpPut("status")]
    public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusRequest request)
    {
        await _libraryService.ChangeStatusAsync(
            request.UserId,
            request.BookId,
            request.Status);

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> Remove(
        [FromQuery] int userId,
        [FromQuery] string bookId)
    {
        await _libraryService.RemoveBookAsync(userId, bookId);
        return Ok();
    }
}