using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult Add([FromBody] CreateReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5 ||
            string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest();
        }

        var review = new Review(
            request.UserId,
            request.OpenLibraryBookId,
            request.BookTitle,
            request.BookAuthor,
            request.CoverId,
            request.Rating,
            request.Text.Trim());

        bool added = _reviewService.AddReview(review);

        return added ? Ok() : Conflict();
    }

    [HttpGet("user/{userId:int}")]
    public ActionResult<List<Review>> GetByUser(int userId)
    {
        return Ok(_reviewService.GetByUser(userId));
    }

    [HttpDelete("{reviewId:int}")]
    public IActionResult Remove(int reviewId, [FromQuery] int userId)
    {
        return _reviewService.RemoveByUser(userId, reviewId)
            ? NoContent()
            : NotFound();
    }


    public record CreateReviewRequest(
        int UserId,
        string OpenLibraryBookId,
        string BookTitle,
        string BookAuthor,
        string? CoverId,
        int Rating,
        string Text);
}