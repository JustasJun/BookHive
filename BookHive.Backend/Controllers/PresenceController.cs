using System.Security.Claims;
using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Backend.Controllers;

[ApiController]
[Route("api/presence")]
[Authorize]
public class PresenceController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;

    public PresenceController(JwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("ticket")]
    public IActionResult CreateTicket()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out int id))
        {
            return Unauthorized();
        }

        string ticket = _jwtTokenService.GeneratePresenceTicket(id);

        return Ok(new PresenceTicketResponse(ticket));
    }
}