using System.Security.Claims;
using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookHive.Backend.DTOs;

namespace BookHive.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    private readonly LibraryService _libraryService;

    public LibraryController(LibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<LibraryBookDetails>>> Get()
    {
        int? userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var books = await _libraryService.GetBookDetailsAsync(userId.Value);
        return Ok(books);
    }

    [HttpGet("ids")]
    public async Task<ActionResult<List<string>>> GetIds()
    {
        int? userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var ids = await _libraryService.GetBookIdsAsync(userId.Value);
        return Ok(ids);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddBookRequest request)
    {
        int? userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        await _libraryService.AddBookAsync(
            userId.Value,
            request.Book,
            request.Status);

        return Ok();
    }

    [HttpPut("status")]
    public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusRequest request)
    {
        int? userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        await _libraryService.ChangeStatusAsync(
            userId.Value,
            request.BookId,
            request.Status);

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> Remove([FromQuery] string bookId)
    {
        int? userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        await _libraryService.RemoveBookAsync(userId.Value, bookId);
        return Ok();
    }

    private int? GetCurrentUserId()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out int id) ? id : null;
    }
}