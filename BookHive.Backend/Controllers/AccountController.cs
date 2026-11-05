using BookHive.Services;
using Microsoft.AspNetCore.Mvc;
using BookHive.Backend.DTOs;
using BookHive.Exceptions;
using BookHive.Models;

namespace BookHive.Backend.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtTokenService _jwtTokenService;

    public AccountController(
        UserService userService,
        JwtTokenService jwtTokenService)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
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
