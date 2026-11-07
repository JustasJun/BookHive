using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;
using BookHive.Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookHive.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class GoalsController : ControllerBase
{
    private readonly GoalService _goalService;

    public GoalsController(GoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpGet]
    public async Task<ActionResult<ReadingGoal>> GetGoal()
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        ReadingGoal? goal = await _goalService.GetGoalAsync(userId.Value, Now());

        return goal == null ? NotFound() : Ok(goal);
    }

    [HttpPost]
    public async Task<IActionResult> SetGoal([FromBody] SetGoalRequest request)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            await _goalService.SetGoalAsync(
                userId.Value,
                request.Period,
                request.TargetBooks,
                Now());

            ReadingGoal? goal = await _goalService.GetGoalAsync(
                userId.Value, Now());

            return Ok(goal);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("progress")]
    public async Task<ActionResult<GoalProgress>> GetProgress()
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        GoalProgress? progress =
            await _goalService.GetProgressAsync(userId.Value, Now());

        return progress == null ? NotFound() : Ok(progress);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<GoalHistoryItem>>> GetHistory()
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        List<GoalHistoryItem> history =
            await _goalService.GetHistoryAsync(userId.Value, Now());

        return Ok(history);
    }

    private int? GetCurrentUserId()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userId, out int id) ? id : null;
    }

    private static DateTime Now() => DateTime.UtcNow;
}