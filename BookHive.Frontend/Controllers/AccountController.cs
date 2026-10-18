using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;
using BookHive.Models;
using BookHive.Frontend.DTOs;

namespace BookHive.Frontend.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    // GET: /Account/
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
    public async Task<IActionResult> Login(string loginInput, string password)
    {
        var client = _httpClientFactory.CreateClient("BackendApi");

        var request = new
        {
            LoginInput = loginInput,
            Password = password
        };

        var response = await client.PostAsJsonAsync(
            "api/account/login",
            request);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Invalid login credentials. Please try again.";
            return View();
        }

        var user = await response.Content.ReadFromJsonAsync<UserDataResponse>();

        if (user == null)
        {
            return View();
        }

        await SignInUserAsync(user);

        var goalResponse = await client.GetAsync($"api/goals?userId={user.Id}");

        bool needsGoal = false;

        if (goalResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // never had a goal
            needsGoal = true;
        }
        else if (goalResponse.IsSuccessStatusCode)
        {
            // had goals before: prompt if the latest one is finished (Completed or Missed)
            var goal = await goalResponse.Content.ReadFromJsonAsync<ReadingGoal>();
            needsGoal = goal == null || goal.Status != GoalStatus.Active;
        }

        if (needsGoal)
        {
            TempData["GoalPrompt"] = true;
        }

        return RedirectToAction("Index", "Home");
    }


    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
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
            var errorData = await response.Content.ReadFromJsonAsync<ErrorResponse>();

            ViewBag.Error = errorData?.Message ?? "An error occurred during registration. Try again.";

            return View();
        }

        var registeredUser = await response.Content.ReadFromJsonAsync<UserDataResponse>();

        if (registeredUser != null)
        {
            await SignInUserAsync(registeredUser);
            TempData["GoalPrompt"] = true;
        }

        return RedirectToAction("Index", "Home");
    }
    public IActionResult UserAccount()
    {
        return View();
    }
    private async Task SignInUserAsync(UserDataResponse user)
    {
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
    }

    // POST: /Account/Logout
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Account");
    }
}