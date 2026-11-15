using BookHive.Services;
using Microsoft.AspNetCore.Mvc;
using BookHive.Backend.DTOs;
using BookHive.Exceptions;
using BookHive.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookHive.Backend.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILibraryService _libraryService;
    private readonly IReviewService _reviewService;
    private readonly IFollowService _followService;

    public AccountController(
        IUserService userService,
        IJwtTokenService jwtTokenService,
        ILibraryService libraryService,
        IReviewService reviewService,
        IFollowService followService)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _libraryService = libraryService;
        _reviewService = reviewService;
        _followService = followService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetCurrentUser()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out int parsedUserId))
        {
            return Unauthorized();
        }

        User? user = await _userService.GetByIdAsync(parsedUserId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(UserProfileResponse.From(user));
    }

    [Authorize]
    [HttpGet("search")]
    public async Task<ActionResult<List<UserSummaryResponse>>> SearchUsers(
        [FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { message = "A search query is required." });
        }

        List<User> users = await _userService.SearchAsync(query.Trim());
        return Ok(users.Select(UserSummaryResponse.From).ToList());
    }

    [Authorize]
    [HttpGet("users/{username}")]
    public async Task<ActionResult<UserAccountResponse>> GetUserProfile(
        string username)
    {
        User? user = await _userService.GetByUsernameAsync(username);
        if (user == null)
        {
            return NotFound();
        }

        var (followers, following) =
            await _followService.GetCountsAsync(user.Id);
        List<Review> reviews = await _reviewService.GetByUserAsync(user.Id);
        List<LibraryBook> books = await _libraryService.GetBooksAsync(user.Id);

        return Ok(new UserAccountResponse(
            UserProfileResponse.From(user),
            books.Count,
            followers,
            following,
            reviews));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _userService.RegisterAsync(
                request.Username,
                request.Email,
                request.Password);

            string token = _jwtTokenService.GenerateToken(user);

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                Token = token
            });
        }
        catch (DuplicateUserException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.LoginAsync(
            request.LoginInput,
            request.Password);

        if (user == null)
        {
            return Unauthorized();
        }

        string token = _jwtTokenService.GenerateToken(user);

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            Token = token
        });
    }
}
