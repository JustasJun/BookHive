using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using BookHive.Frontend.DTOs;
using BookHive.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Frontend.Controllers;

[Authorize]
[Route("Profile")]
public class ProfileController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProfileController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("SearchUsers")]
    public async Task<IActionResult> SearchUsers(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { message = "A search query is required." });
        }

        var client = _httpClientFactory.CreateClient("BackendApi");
        string url = $"api/account/search?query={Uri.EscapeDataString(query.Trim())}";
        using HttpResponseMessage response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Could not search for users. Please try again later.");
        }

        List<UserSummaryResponse> users =
            await response.Content.ReadFromJsonAsync<List<UserSummaryResponse>>()
            ?? throw new InvalidOperationException(
                "The user search service returned an empty response.");

        string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var results = users
            .Where(user => user.Id.ToString() != currentUserId)
            .Select(user => new
            {
                user.Username,
                user.DisplayName,
                user.AvatarUrl,
                ProfileUrl = Url.Action(
                    nameof(ViewProfile),
                    values: new { username = user.Username })
            });

        return Json(results);
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> ViewProfile(string username)
    {
        var client = _httpClientFactory.CreateClient("BackendApi");
        using HttpResponseMessage profileResponse = await client.GetAsync(
            $"api/account/users/{Uri.EscapeDataString(username)}");

        if (profileResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!profileResponse.IsSuccessStatusCode)
        {
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Could not load this profile. Please try again later.");
        }

        UserAccountResponse? profile =
            await profileResponse.Content.ReadFromJsonAsync<UserAccountResponse>();
        if (profile == null)
        {
            throw new InvalidOperationException(
                "The profile service returned an empty response.");
        }

        string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool isOwner = int.TryParse(currentUserId, out int parsedUserId) &&
            parsedUserId == profile.Profile.Id;
        bool isFollowing = false;

        if (!isOwner)
        {
            using HttpResponseMessage followingResponse = await client.GetAsync(
                $"api/follows/{profile.Profile.Id}/is-following");

            if (!followingResponse.IsSuccessStatusCode)
            {
                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "Could not load follow status. Please try again later.");
            }

            isFollowing = await followingResponse.Content.ReadFromJsonAsync<bool>();
        }

        return View("~/Views/Account/UserAccount.cshtml", new UserAccountViewModel
        {
            Profile = profile.Profile,
            BookCount = profile.BookCount,
            FollowersCount = profile.FollowersCount,
            FollowingCount = profile.FollowingCount,
            Reviews = profile.Reviews,
            IsOwner = isOwner,
            IsFollowing = isFollowing
        });
    }

    [HttpPost("{username}/Follow")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Follow(string username)
    {
        var client = _httpClientFactory.CreateClient("BackendApi");
        using HttpResponseMessage userResponse = await client.GetAsync(
            $"api/account/users/{Uri.EscapeDataString(username)}");

        if (!userResponse.IsSuccessStatusCode)
        {
            TempData["ProfileError"] = "Could not find this user.";
            return RedirectToAction(nameof(ViewProfile), new { username });
        }

        UserAccountResponse? user =
            await userResponse.Content.ReadFromJsonAsync<UserAccountResponse>();
        if (user == null)
        {
            throw new InvalidOperationException(
                "The profile service returned an empty response.");
        }

        using HttpResponseMessage response = await client.PostAsync(
            $"api/follows/{user.Profile.Id}",
            content: null);

        if (!response.IsSuccessStatusCode)
        {
            TempData["ProfileError"] = "Could not follow this user. Please try again.";
        }

        return RedirectToAction(nameof(ViewProfile), new { username });
    }

    [HttpPost("{username}/Unfollow")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unfollow(string username)
    {
        var client = _httpClientFactory.CreateClient("BackendApi");
        using HttpResponseMessage userResponse = await client.GetAsync(
            $"api/account/users/{Uri.EscapeDataString(username)}");

        if (!userResponse.IsSuccessStatusCode)
        {
            TempData["ProfileError"] = "Could not find this user.";
            return RedirectToAction(nameof(ViewProfile), new { username });
        }

        UserAccountResponse? user =
            await userResponse.Content.ReadFromJsonAsync<UserAccountResponse>();
        if (user == null)
        {
            throw new InvalidOperationException(
                "The profile service returned an empty response.");
        }

        using HttpResponseMessage response = await client.DeleteAsync(
            $"api/follows/{user.Profile.Id}");

        if (!response.IsSuccessStatusCode)
        {
            TempData["ProfileError"] = "Could not unfollow this user. Please try again.";
        }

        return RedirectToAction(nameof(ViewProfile), new { username });
    }
}
