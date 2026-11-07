using BookHive.Models;
using BookHive.Services;
using BookHive.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using BookHive.Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookHive.Backend.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly ReviewService _reviewService;

    public ReviewsController(ReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add([FromBody] CreateReviewRequest request)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        AddReviewResult result = await _reviewService.AddReviewAsync(
            userId.Value,
            request.OpenLibraryBookId,
            request.BookTitle,
            request.BookAuthor,
            request.CoverId,
            request.Rating,
            request.Text);

        return result switch
        {
            AddReviewResult.Added => Ok(),
            AddReviewResult.AlreadyExists => Conflict(),
            _ => BadRequest()
        };
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<List<Review>>> GetByUser(int userId)
    {
        List<Review> reviews = await _reviewService.GetByUserAsync(userId);
        return Ok(reviews);
    }

    [HttpDelete("{reviewId:int}")]
    [Authorize]
    public async Task<IActionResult> Remove(int reviewId)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        return await _reviewService.RemoveByUserAsync(userId.Value, reviewId)
            ? NoContent()
            : NotFound();
    }

    private int? GetCurrentUserId()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userId, out int id) ? id : null;
    }

    [HttpGet("community")]
    [Authorize]
    public ActionResult<List<CommunityReviewItem>> GetCommunityFeed()
    {
        return Ok(_reviewService.GetCommunityFeed());
    }
}