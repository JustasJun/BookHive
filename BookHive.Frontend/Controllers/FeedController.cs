using Microsoft.AspNetCore.Mvc;

namespace BookHive.Frontend.Controllers;

public class FeedController : Controller
{
    public IActionResult Index() => View();
}