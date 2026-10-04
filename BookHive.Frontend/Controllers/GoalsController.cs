using System.Net.Http.Json;
using System.Security.Claims;
using BookHive.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Frontend.Controllers;

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

        var response = await client.GetAsync($"api/goals/progress?userId={userId}");

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
                year,
                targetBooks
            });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Could not set reading goal.");
            ViewData["Edit"] = true;
            return View("Index", null);
        }

        return RedirectToAction(nameof(Index));
    }
}