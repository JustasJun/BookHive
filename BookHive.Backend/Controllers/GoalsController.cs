using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoalsController : ControllerBase
{
    private readonly GoalService _goalService;
    private readonly ReviewService _reviewService;

    public GoalsController(GoalService goalService, ReviewService reviewService)
    {
        _goalService = goalService;
        _reviewService = reviewService;
    }

    [HttpGet]
    public ActionResult GetGoal([FromQuery] int userId)
    {
        var goal = _goalService.GetGoal(userId);

        if (goal == null)
        {
            return NotFound();
        }

        return Ok(goal);
    }

    [HttpPost]
    public IActionResult SetGoal([FromBody] SetGoalRequest request)
    {
        if (request.TargetBooks <= 0)
        {
            return BadRequest();
        }

        _goalService.SetGoal(request.UserId, request.Year, request.TargetBooks);

        return Ok(_goalService.GetGoal(request.UserId));
    }

    [HttpGet("progress")]
    public ActionResult GetProgress([FromQuery] int userId)
    {
        var goal = _goalService.GetGoal(userId);

        if (goal == null)
        {
            return NotFound();
        }

        var booksRead = _reviewService.CountForGoal(userId, goal);

        var progress = _goalService.Calculate(
            goal,
            booksRead,
            DateOnly.FromDateTime(DateTime.Today));

        return Ok(progress);
    }
}

public record SetGoalRequest(
    int UserId,
    int Year,
    int TargetBooks);