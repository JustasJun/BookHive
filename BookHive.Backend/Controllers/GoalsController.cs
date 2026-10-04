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

        var booksRead = _libraryService.CountReadForGoal(userId, goal);

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