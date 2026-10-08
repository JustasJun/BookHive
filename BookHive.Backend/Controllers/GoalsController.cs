using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoalsController : ControllerBase
{
    private readonly GoalService _goalService;
    private readonly LibraryService _libraryService;

    public GoalsController(GoalService goalService, LibraryService libraryService)
    {
        _goalService = goalService;
        _libraryService = libraryService;
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
        ReadingGoal? goal = _goalService.GetGoal(userId);

        if (goal == null)
        {
            return NotFound();
        }

        int booksRead = _libraryService.CountReadForGoal(userId, goal);
        GoalProgress progress = _goalService.Calculate(goal, booksRead, DateOnly.FromDateTime(DateTime.Today));

        return Ok(progress);
    }
}

public record SetGoalRequest(int UserId, int Year, int TargetBooks);