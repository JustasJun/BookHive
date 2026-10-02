using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
    public async Task<IActionResult> Login(string email, string password)
    {
        var client = _httpClientFactory.CreateClient("BackendApi");

        var request = new
        {
            Email = email,
            Password = password
        };

        var response = await client.PostAsJsonAsync(
            "api/account/login",
            request);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Neteisingas el. paštas arba slaptažodis.";
            return View();
        }

        var user = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (user == null)
        {
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

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

public record LoginResponse(
    int Id,
    string Username,
    string Email);