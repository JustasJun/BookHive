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
    public async Task<ActionResult<ReadingGoal>> GetGoal([FromQuery] int userId)
    {
        ReadingGoal? goal = await _goalService.GetGoalAsync(userId, Now());

        return goal == null ? NotFound() : Ok(goal);
    }

    [HttpPost]
    public async Task<IActionResult> SetGoal([FromBody] SetGoalRequest request)
    {
        try
        {
            await _goalService.SetGoalAsync(
                request.UserId,
                request.Period,
                request.TargetBooks,
                Now());

            ReadingGoal? goal = await _goalService.GetGoalAsync(
                request.UserId, Now());

            return Ok(goal);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("progress")]
    public async Task<ActionResult<GoalProgress>> GetProgress([FromQuery] int userId)
    {
        GoalProgress? progress =
            await _goalService.GetProgressAsync(userId, Now());

        return progress == null ? NotFound() : Ok(progress);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<GoalHistoryItem>>> GetHistory(
        [FromQuery] int userId)
    {
        List<GoalHistoryItem> history =
            await _goalService.GetHistoryAsync(userId, Now());

        return Ok(history);
    }

    private static DateTime Now() => DateTime.UtcNow;
}