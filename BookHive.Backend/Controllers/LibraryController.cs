using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Backend.Controllers;

[ApiController]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    private readonly LibraryService _libraryService;
    private readonly OpenLibraryClient _client;

    public LibraryController(LibraryService libraryService, OpenLibraryClient client)
    {
        _libraryService = libraryService;
        _client = client;
    }

    [HttpGet("{userId}")]
   public async Task<ActionResult<List<LibraryBookDetails>>> Get(int userId)
    {
        var libraryBooks = _libraryService.GetBooks(userId);
        var result = new List<LibraryBookDetails>();

        foreach (var item in libraryBooks)
        {
            Book? book = await _client.GetByIdAsync(item.OpenLibraryBookId);

            if (book != null)
            {
                result.Add(new LibraryBookDetails(book, item.Status, item.IsFavorite));
            }
        }

        return Ok(result);
    }

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