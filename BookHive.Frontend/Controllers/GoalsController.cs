using System.Net.Http.Json;
using System.Security.Claims;
using BookHive.Frontend.Models;
using BookHive.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BookHive.Frontend.Controllers;

[Authorize]
public class GoalsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GoalsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index(bool edit = false)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["Edit"] = edit;

        var client = _httpClientFactory.CreateClient("BackendApi");
        var model = new GoalsPageViewModel();

        var progressResponse = await client.GetAsync($"api/goals/progress?userId={userId}");
        if (progressResponse.IsSuccessStatusCode)
        {
            model.Current = await progressResponse.Content.ReadFromJsonAsync<GoalProgress>();
        }

        var historyResponse = await client.GetAsync($"api/goals/history?userId={userId}");
        if (historyResponse.IsSuccessStatusCode)
        {
            model.History = await historyResponse.Content.ReadFromJsonAsync<List<GoalHistoryItem>>()
                ?? new List<GoalHistoryItem>();
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SetGoal(GoalPeriod period, int targetBooks)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var client = _httpClientFactory.CreateClient("BackendApi");

        var response = await client.PostAsJsonAsync(
            "api/goals",
            new
            {
                userId = int.Parse(userId),
                period,
                targetBooks
            });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Could not set reading goal.");
            ViewData["Edit"] = true;
            return View("Index", new GoalsPageViewModel());
        }

        return RedirectToAction(nameof(Index));
    }
}
