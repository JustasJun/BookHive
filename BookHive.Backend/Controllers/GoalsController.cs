using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoalsController : ControllerBase
{
    private readonly GoalService _goalService;

    public GoalsController(GoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpGet]
    public ActionResult<ReadingGoal> GetGoal([FromQuery] int userId)
    {
        ReadingGoal? goal = _goalService.GetGoal(userId);

        return goal == null ? NotFound() : Ok(goal);
    }

    [HttpPost]
    public IActionResult SetGoal([FromBody] SetGoalRequest request)
    {
        try
        {
            _goalService.SetGoal(request.UserId, request.Year, request.TargetBooks);
            return Ok(_goalService.GetGoal(request.UserId));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("progress")]
    public ActionResult<GoalProgress> GetProgress([FromQuery] int userId)
    {
        GoalProgress? progress = _goalService.GetProgress(userId, DateOnly.FromDateTime(DateTime.Today));

        return progress == null ? NotFound() : Ok(progress);
    }
}

public record SetGoalRequest(int UserId, int Year, int TargetBooks);