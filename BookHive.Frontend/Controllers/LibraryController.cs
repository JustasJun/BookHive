using System.Net.Http.Json;
using BookHive.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BookHive.Frontend.Models;

namespace BookHive.Frontend.Controllers;

[Authorize]
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
            var books = await client.GetFromJsonAsync<List<LibraryBookDetails>>("api/library");
            return View(books ?? new List<LibraryBookDetails>());
        }
        catch (Exception)
        {
            return View(new List<LibraryBookDetails>());
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
                bookId,
                status = "Read"
            });

        if (response.IsSuccessStatusCode)
        {
            var goalResponse = await client.GetAsync("api/goals/progress");

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
    
    [HttpGet]
    public async Task<IActionResult> GetIds()
    {
        var client = _httpClientFactory.CreateClient("BackendApi");
        var response = await client.GetAsync("api/library/ids");

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode);
        }

        var ids = await response.Content.ReadFromJsonAsync<List<string>>();
        return Json(ids ?? new List<string>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add([FromBody] AddLibraryBookModel model)
    {
        var client = _httpClientFactory.CreateClient("BackendApi");

        var response = await client.PostAsJsonAsync("api/library", new
        {
            model.Book,
            model.Status
        });

        return StatusCode((int)response.StatusCode);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(string bookId)
    {
        if (string.IsNullOrWhiteSpace(bookId))
        {
            return BadRequest();
        }

        var client = _httpClientFactory.CreateClient("BackendApi");

        var response = await client.DeleteAsync(
            $"api/library?bookId={Uri.EscapeDataString(bookId)}");

        return StatusCode((int)response.StatusCode);
    }

}
