using System.Net;
using System.Net.Http.Json;
using BookHive.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Frontend.Controllers;

[Authorize]
public class CommunityController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CommunityController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        HttpClient client = _httpClientFactory.CreateClient("BackendApi");

        using HttpResponseMessage response =
            await client.GetAsync("api/reviews/community");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Could not load community activity. Try again later.";
            Response.StatusCode = (int)HttpStatusCode.BadGateway;
            return View(new List<CommunityReviewItem>());
        }

        List<CommunityReviewItem>? items =
            await response.Content.ReadFromJsonAsync<List<CommunityReviewItem>>();

        return View(items ?? new List<CommunityReviewItem>());
    }
}