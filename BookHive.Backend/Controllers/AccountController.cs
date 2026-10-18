using BookHive.Services;
using Microsoft.AspNetCore.Mvc;
using BookHive.Backend.DTOs;

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
        try
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _userService.Login(
            request.LoginInput,
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
