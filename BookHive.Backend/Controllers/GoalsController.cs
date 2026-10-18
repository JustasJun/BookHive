using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;
using BookHive.Backend.DTOs;

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
        ReadingGoal? goal = _goalService.GetGoal(userId, Now());

        return goal == null ? NotFound() : Ok(goal);
    }

    [HttpPost]
    public IActionResult SetGoal([FromBody] SetGoalRequest request)
    {
        try
        {
            _goalService.SetGoal(request.UserId, request.Period, request.TargetBooks, Now());
            return Ok(_goalService.GetGoal(request.UserId, Now()));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("progress")]
    public ActionResult<GoalProgress> GetProgress([FromQuery] int userId)
    {
        GoalProgress? progress = _goalService.GetProgress(userId, Now());

        return progress == null ? NotFound() : Ok(progress);
    }

    [HttpGet("history")]
    public ActionResult<List<GoalHistoryItem>> GetHistory([FromQuery] int userId)
    {
        return Ok(_goalService.GetHistory(userId, Now()));
    }

    private static DateTime Now() => DateTime.UtcNow;
}