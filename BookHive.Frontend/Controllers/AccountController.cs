using Microsoft.AspNetCore.Mvc;

namespace BookHive.Frontend.Controllers;

public class AccountController : Controller
{
    // GET: /Account/
    // Pagrindinis puslapis su mygtukais "Prisijungti" ir "Registruotis"
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        // Vėliau čia skaityt 'email' bei 'password'
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    // Tiesioginis nukreipimas be tikrinimo
    [HttpPost]
    public IActionResult Register(string username, string email, string password)
    {
        // Vėliau čia išsaugti vartotoją
        return RedirectToAction("Index", "Home");
    }
}