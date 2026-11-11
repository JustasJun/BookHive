using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookHive.Models;
using Microsoft.IdentityModel.Tokens;

namespace BookHive.Services;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return CreateToken(claims, TimeSpan.FromHours(2));
    }

    public string GeneratePresenceTicket(int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(PresenceAuth.PurposeClaim, PresenceAuth.PresencePurpose),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return CreateToken(claims, PresenceAuth.TicketLifetime);
    }

    private string CreateToken(IEnumerable<Claim> claims, TimeSpan lifetime)
    {
        string key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is not configured.");

        string issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is not configured.");

        string audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is not configured.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}