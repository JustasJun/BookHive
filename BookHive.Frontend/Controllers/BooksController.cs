using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BookHive.Frontend.Controllers;

[Authorize]
public class BooksController : Controller
{
    public IActionResult Index() => View();
}