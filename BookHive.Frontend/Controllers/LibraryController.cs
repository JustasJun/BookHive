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

        var client = _httpClientFactory.CreateClient("BackendApi");

        var books = await client.GetFromJsonAsync<List<LibraryBook>>(
            $"api/library/{userId}");

        return View(books ?? new List<LibraryBook>());
    }
}