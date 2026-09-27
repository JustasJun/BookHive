using Microsoft.AspNetCore.Mvc;

namespace BookHive.Frontend.Controllers;

public class BooksController : Controller
{
    public IActionResult Index() => View();
}