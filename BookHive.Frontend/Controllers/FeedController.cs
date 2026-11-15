using System.Net;
using System.Net.Http.Json;
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
        var client = _httpClientFactory.CreateClient("BackendApi");
        var response = await client.GetAsync("api/reviews/feed");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Could not load your feed. Try again later.";
            Response.StatusCode = (int)HttpStatusCode.BadGateway;
            return View(new List<CommunityReviewItem>());
        }

        var items = await response.Content.ReadFromJsonAsync<List<CommunityReviewItem>>();
        return View(items ?? new List<CommunityReviewItem>());
    }
}
