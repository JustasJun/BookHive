using Microsoft.AspNetCore.Mvc;

namespace BookHive.Frontend.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult About()
    {
        return View("PageAboutUs");
    }
}