using BookHive.Backend.Controllers;
using BookHive.Models;
using BookHive.Services;
using BookHive.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BookHive.Tests.Controllers;

public class PresenceControllerTests
{
    private static PresenceController CreateController(int? userId)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-test-key-test-key-test-key-1234",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            })
            .Build();

        return new PresenceController(new JwtTokenService(configuration))
            .WithUser(userId);
    }

    [Fact]
    public void CreateTicket_WhenNotLoggedIn_ReturnsUnauthorized()
    {
        IActionResult result = CreateController(userId: null).CreateTicket();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void CreateTicket_WhenLoggedIn_ReturnsTicket()
    {
        IActionResult result = CreateController(userId: 1).CreateTicket();

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PresenceTicketResponse>(ok.Value);
        Assert.False(string.IsNullOrWhiteSpace(response.Ticket));
    }
}