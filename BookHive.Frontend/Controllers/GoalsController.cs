using System.Net.Http.Json;
using BookHive.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Controllers;

public class GoalsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GoalsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("BackendApi");

        var response = await client.GetAsync("api/goals/progress");

        if (!response.IsSuccessStatusCode)
        {
            return View(null);
        }

        var progress = await response.Content.ReadFromJsonAsync<GoalProgress>();

        return View(progress);
    }

    [HttpPost]
    public async Task<IActionResult> SetGoal(int year, int targetBooks)
    {
        var client = _httpClientFactory.CreateClient("BackendApi");

        var response = await client.PostAsJsonAsync(
            "api/goals",
            new
            {
                year,
                targetBooks
            });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Could not set reading goal.");
            return View("Index", null);
        }

        return RedirectToAction(nameof(Index));
    }
}