using BookHive.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Backend.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly UserService _userService;

    public AccountController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        var user = _userService.Register(
            request.Username,
            request.Email,
            request.Password);

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email
        });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _userService.Login(
            request.Email,
            request.Password);

        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email
        });
    }
}

public record RegisterRequest(
    string Username,
    string Email,
    string Password);

public record LoginRequest(
    string Email,
    string Password);