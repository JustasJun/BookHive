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

        return Ok(user);
    }
}

public record RegisterRequest(
    string Username,
    string Email,
    string Password);