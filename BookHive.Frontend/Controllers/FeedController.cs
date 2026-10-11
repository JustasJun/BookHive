using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using BookHive.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Frontend.Controllers;

[Authorize]
public class FeedController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FeedController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out int parsedUserId))
        {
            return Unauthorized();
        }

        var client = _httpClientFactory.CreateClient("BackendApi");
        var response = await client.GetAsync($"api/reviews/user/{parsedUserId}");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Could not load your feed. Try again later.";
            Response.StatusCode = (int)HttpStatusCode.BadGateway;
            return View(new List<Review>());
        }

        var reviews = await response.Content.ReadFromJsonAsync<List<Review>>();
        return View(reviews ?? new List<Review>());
    }
}