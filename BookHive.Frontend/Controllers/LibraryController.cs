using System.Net.Http.Json;
using BookHive.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookHive.Frontend.Controllers;

public class LibraryController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LibraryController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            var books = await client.GetFromJsonAsync<List<LibraryBook>>(
                $"api/library/{userId}");

            return View(books ?? new List<LibraryBook>());
        }
        catch (Exception ex)
        {
            return View(new List<LibraryBook>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(string bookId)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var client = _httpClientFactory.CreateClient("BackendApi");

        var response = await client.PutAsJsonAsync(
            "api/library/status",
            new
            {
                userId = int.Parse(userId),
                bookId,
                status = "Read"
            });

        if (response.IsSuccessStatusCode)
        {
            var goalResponse = await client.GetAsync($"api/goals/progress?userId={userId}");

            if (goalResponse.IsSuccessStatusCode)
            {
                var progress = await goalResponse.Content.ReadFromJsonAsync<GoalProgress>();

                if (progress != null)
                {
                    TempData["GoalRead"] = progress.BooksRead;
                    TempData["GoalTarget"] = progress.TargetBooks;
                }
            }
        }

        return RedirectToAction(nameof(Index));
    }

    
}