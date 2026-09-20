using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Controllers;

public class LibraryController : Controller
{
    private readonly LibraryService _libraryService;

    public LibraryController(LibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    public IActionResult Index()
    {
        var books = _libraryService.GetBooks();
        return View(books);
    }
}