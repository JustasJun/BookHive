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
    public ActionResult<List<LibraryBook>> Get(int userId)
    {
        return Ok(_libraryService.GetBooks(userId));
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

    public record AddBookRequest(
        int UserId,
        Book Book,
        ReadingStatus Status);

}