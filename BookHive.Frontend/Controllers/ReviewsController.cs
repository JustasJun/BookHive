using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using BookHive.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using BookHive.Models;
using Microsoft.AspNetCore.Authorization;

namespace BookHive.Frontend.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ReviewsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Write(string bookId, string title, string author, string? coverId)
    {
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) == null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(new WriteReviewViewModel
        {
            BookId = bookId,
            Title = title,
            Author = author,
            CoverId = coverId
        });
    }

    [HttpPost]
    public async Task<IActionResult> Write(WriteReviewViewModel model)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _httpClientFactory.CreateClient("BackendApi");
        var response = await client.PostAsJsonAsync("api/reviews", new
        {
            UserId = int.Parse(userId),
            OpenLibraryBookId = model.BookId,
            BookTitle = model.Title,
            BookAuthor = model.Author,
            model.CoverId,
            model.Rating,
            model.Text
        });

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            ModelState.AddModelError("", "You have already reviewed this book.");
            return View(model);
        }

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Could not save the review. Try again.");
            return View(model);
        }

        return RedirectToAction("Index", "Feed");
    }

    [HttpGet]
    public async Task<IActionResult> UserPosts()
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
            ViewBag.Error = "Could not load your posts. Try again later.";
            Response.StatusCode = (int)HttpStatusCode.BadGateway;
            return View(new List<Review>());
        }

        var reviews = await response.Content.ReadFromJsonAsync<List<Review>>();
        return View(reviews ?? new List<Review>());
    }

}