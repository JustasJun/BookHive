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
    public ActionResult GetGoal()
    {
        var goal = _goalService.GetGoal();

        if (goal == null)
            return NotFound();

        return Ok(goal);
    }

    [HttpPost]
    public IActionResult SetGoal([FromBody] SetGoalRequest request)
    {
        _goalService.SetGoal(request.Year, request.TargetBooks);

        return Ok(_goalService.GetGoal());
    }

    [HttpGet("progress")]
    public ActionResult GetProgress()
    {
        var goal = _goalService.GetGoal();

        if (goal == null)
            return NotFound();

        // Kol kas naudojame 0, nes tikras booksRead
        // bus prijungtas prie Library vėliau.
        var booksRead = 0;

        var progress = _goalService.Calculate(
            goal,
            booksRead,
            DateOnly.FromDateTime(DateTime.Today));

        return Ok(progress);
    }
}

public record SetGoalRequest(
    int Year,
    int TargetBooks);