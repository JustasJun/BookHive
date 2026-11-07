using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;
using BookHive.Models;
using BookHive.Frontend.DTOs;
using BookHive.Frontend.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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

        using HttpRequestMessage goalRequest = new(
            HttpMethod.Get,
            "api/goals");

        goalRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", user.Token);

        using HttpResponseMessage goalResponse =
            await client.SendAsync(goalRequest);

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
    [Authorize]
    public async Task<IActionResult> UserAccount()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out int parsedUserId))
        {
            return Unauthorized();
        }

        var client = _httpClientFactory.CreateClient("BackendApi");
        Task<HttpResponseMessage> profileTask = client.GetAsync("api/account/me");
        Task<HttpResponseMessage> bookIdsTask = client.GetAsync("api/library/ids");
        Task<HttpResponseMessage> reviewsTask =
            client.GetAsync($"api/reviews/user/{parsedUserId}");
        Task<HttpResponseMessage> followCountsTask =
            client.GetAsync($"api/follows/{parsedUserId}/counts");

        await Task.WhenAll(profileTask, bookIdsTask, reviewsTask, followCountsTask);

        using HttpResponseMessage profileResponse = await profileTask;
        using HttpResponseMessage bookIdsResponse = await bookIdsTask;
        using HttpResponseMessage reviewsResponse = await reviewsTask;
        using HttpResponseMessage followCountsResponse = await followCountsTask;

        if (!profileResponse.IsSuccessStatusCode ||
            !bookIdsResponse.IsSuccessStatusCode ||
            !reviewsResponse.IsSuccessStatusCode ||
            !followCountsResponse.IsSuccessStatusCode)
        {
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Could not load your profile. Please try again later.");
        }

        UserProfileResponse? profile =
            await profileResponse.Content.ReadFromJsonAsync<UserProfileResponse>();
        List<string>? bookIds =
            await bookIdsResponse.Content.ReadFromJsonAsync<List<string>>();
        List<Review>? reviews =
            await reviewsResponse.Content.ReadFromJsonAsync<List<Review>>();
        FollowCountsResponse? followCounts =
            await followCountsResponse.Content.ReadFromJsonAsync<FollowCountsResponse>();

        if (profile == null || bookIds == null || reviews == null ||
            followCounts == null)
        {
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "The profile service returned incomplete data.");
        }

        return View(new UserAccountViewModel
        {
            Profile = profile,
            BookCount = bookIds.Count,
            FollowersCount = followCounts.Followers,
            FollowingCount = followCounts.Following,
            Reviews = reviews
        });
    }
    
    private async Task SignInUserAsync(UserDataResponse user)
    {
        if (string.IsNullOrWhiteSpace(user.Token))
        {
            throw new InvalidOperationException(
                "Backend did not provide an authentication token.");
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

        var properties = new AuthenticationProperties
        {
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2),
            AllowRefresh = false,
            IsPersistent = false
        };

        properties.StoreTokens(
        [
            new AuthenticationToken
            {
                Name = "access_token",
                Value = user.Token
            }
        ]);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);
    }


    // POST: /Account/Logout
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Account");
    }
}
