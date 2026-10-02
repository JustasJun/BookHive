using System.Net.Http.Json;
using BookHive.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Frontend.Controllers;

public class LibraryController : Controller
{
    private const int CurrentUserId = 1;
    private readonly IHttpClientFactory _httpClientFactory;

    public LibraryController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendApi");
            var books = await client.GetFromJsonAsync<List<LibraryBook>>($"api/library/{CurrentUserId}");
         return View(books ?? new List<LibraryBook>());
        }
        catch (Exception ex)
        {
            return View(new List<LibraryBook>());
        }
    }
}