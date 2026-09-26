using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Controllers;

public class LibraryController : Controller
{
    // TODO: kai bus prisijungimas, imti iš User.Identity, o ne fiksuoti
    private const int CurrentUserId = 1;
    private readonly LibraryService _libraryService;

    public LibraryController(LibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    public IActionResult Index()
    {
        var books = _libraryService.GetBooks(CurrentUserId);
        return View(books);
    }
}