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
    public IActionResult Add([FromBody] CreateReviewRequest request)
    {
        AddReviewResult result = _reviewService.AddReview(
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
}