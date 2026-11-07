using System.Security.Claims;
using BookHive.Backend.DTOs;
using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/follows")]
public class FollowsController : ControllerBase
{
    private readonly FollowService _followService;

    public FollowsController(FollowService followService)
    {
        _followService = followService;
    }

    // current user follows {userId}
    [HttpPost("{userId:int}")]
    public async Task<IActionResult> Follow(int userId)
    {
        int? currentUserId = GetCurrentUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        try
        {
            await _followService.FollowAsync(currentUserId.Value, userId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // current user unfollows {userId}
    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> Unfollow(int userId)
    {
        int? currentUserId = GetCurrentUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        await _followService.UnfollowAsync(currentUserId.Value, userId);
        return NoContent();
    }

    // does the current user follow {userId}?
    [HttpGet("{userId:int}/is-following")]
    public async Task<ActionResult<bool>> IsFollowing(int userId)
    {
        int? currentUserId = GetCurrentUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        return Ok(await _followService.IsFollowingAsync(
            currentUserId.Value, userId));
    }

    [HttpGet("{userId:int}/followers")]
    public async Task<ActionResult<List<UserSummaryResponse>>> GetFollowers(
        int userId)
    {
        try
        {
            List<User> followers = await _followService.GetFollowersAsync(userId);
            return Ok(followers.Select(UserSummaryResponse.From).ToList());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{userId:int}/following")]
    public async Task<ActionResult<List<UserSummaryResponse>>> GetFollowing(
        int userId)
    {
        try
        {
            List<User> following = await _followService.GetFollowingAsync(userId);
            return Ok(following.Select(UserSummaryResponse.From).ToList());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{userId:int}/counts")]
    public async Task<ActionResult<FollowCountsResponse>> GetCounts(int userId)
    {
        try
        {
            var (followers, following) = await _followService.GetCountsAsync(userId);
            return Ok(new FollowCountsResponse(followers, following));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private int? GetCurrentUserId()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userId, out int id) ? id : null;
    }
}