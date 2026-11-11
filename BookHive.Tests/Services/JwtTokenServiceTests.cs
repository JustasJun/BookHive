using System.IdentityModel.Tokens.Jwt;
using BookHive.Models;
using BookHive.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BookHive.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-test-key-test-key-test-key-1234",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            })
            .Build();

        _service = new JwtTokenService(configuration);
    }

    private static JwtSecurityToken Read(string token) =>
        new JwtSecurityTokenHandler().ReadJwtToken(token);

    [Fact]
    public void GeneratePresenceTicket_ContainsUserIdAndPresencePurpose()
    {
        JwtSecurityToken ticket = Read(_service.GeneratePresenceTicket(5));

        Assert.Equal("5", ticket.Subject);
        Assert.Contains(ticket.Claims, c =>
            c.Type == PresenceAuth.PurposeClaim &&
            c.Value == PresenceAuth.PresencePurpose);
    }

    [Fact]
    public void GeneratePresenceTicket_IsShortLived()
    {
        JwtSecurityToken ticket = Read(_service.GeneratePresenceTicket(5));

        Assert.True(ticket.ValidTo > DateTime.UtcNow);
        Assert.True(ticket.ValidTo <= DateTime.UtcNow.AddMinutes(2));
    }

    [Fact]
    public void GenerateToken_HasNoPresencePurpose()
    {
        var user = new User("meda", "meda@test.com", "hash") { Id = 5 };

        JwtSecurityToken token = Read(_service.GenerateToken(user));

        Assert.DoesNotContain(token.Claims, c => c.Type == PresenceAuth.PurposeClaim);
        Assert.True(token.ValidTo > DateTime.UtcNow.AddHours(1));
    }
}