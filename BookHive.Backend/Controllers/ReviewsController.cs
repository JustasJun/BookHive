using BookHive.Models;
using BookHive.Services;
using BookHive.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using BookHive.Backend.DTOs;

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
    public async Task<IActionResult> Add([FromBody] CreateReviewRequest request)
    {
        var result = await _reviewService.AddReviewAsync(
            request.UserId,
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
        var reviews = await _reviewService.GetByUserAsync(userId);
        return Ok(reviews);
    }

    [HttpDelete("{reviewId:int}")]
    public async Task<IActionResult> Remove(int reviewId, [FromQuery] int userId)
    {
        return await _reviewService.RemoveByUserAsync(userId, reviewId)
            ? NoContent()
            : NotFound();
    }
}