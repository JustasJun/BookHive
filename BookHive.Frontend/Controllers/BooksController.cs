using Microsoft.AspNetCore.Mvc;

namespace BookHive.Controllers;

public class BooksController : Controller
{
    public IActionResult Index() => View();
}