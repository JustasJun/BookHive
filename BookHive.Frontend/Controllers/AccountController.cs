using Microsoft.AspNetCore.Mvc;

namespace BookHive.Frontend.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
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
    public async Task<IActionResult> Register(
        string username,
        string email,
        string password)
    {
        var client = _httpClientFactory.CreateClient("BackendApi");

        var request = new
        {
            Username = username,
            Email = email,
            Password = password
        };

        var response = await client.PostAsJsonAsync(
            "api/account/register",
            request);

        if (!response.IsSuccessStatusCode)
        {
            return View();
        }

        return RedirectToAction("Index", "Home");
    }

    public IActionResult UserAccount()
    {
        return View();
    }
}