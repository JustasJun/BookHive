using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using BookHive.Models;
using BookHive.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Json;

namespace BookHive.Tests.Integration;

public class JwtAuthorizationTests
{
    private const string JwtKey =
        "BookHive-Integration-Test-Key-12345678901234567890";

    private const string JwtIssuer = "BookHive.Tests";
    private const string JwtAudience = "BookHive.Tests.Client";

    private const string LibraryEndpoint = "/api/library/ids";
    private const string TicketEndpoint = "/api/presence/ticket";

    private sealed class BackendFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Jwt:Key", JwtKey);
            builder.UseSetting("Jwt:Issuer", JwtIssuer);
            builder.UseSetting("Jwt:Audience", JwtAudience);
        }
    }

    private static HttpClient CreateClient(BackendFactory factory)
    {
        return factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    private static string CreateToken(
        string? key = null,
        string? issuer = null,
        string? audience = null,
        DateTime? expires = null,
        IEnumerable<Claim>? additionalClaims = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "42"),
            new(JwtRegisteredClaimNames.UniqueName, "testuser"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (additionalClaims != null)
        {
            claims.AddRange(additionalClaims);
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key ?? JwtKey));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer ?? JwtIssuer,
            audience: audience ?? JwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddHours(-2),
            expires: expires ?? DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static HttpRequestMessage CreateRequest(
        HttpMethod method,
        string endpoint,
        string? token = null)
    {
        var request = new HttpRequestMessage(method, endpoint);

        if (token != null)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return request;
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new BackendFactory();
        using var client = CreateClient(factory);

        using var response = await client.GetAsync(LibraryEndpoint);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithMalformedToken_ReturnsUnauthorized()
    {
        using var factory = new BackendFactory();
        using var client = CreateClient(factory);

        using var request = CreateRequest(
            HttpMethod.Get,
            LibraryEndpoint,
            "invalid-jwt-token");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithExpiredToken_ReturnsUnauthorized()
    {
        using var factory = new BackendFactory();
        using var client = CreateClient(factory);

        var token = CreateToken(
            expires: DateTime.UtcNow.AddHours(-1));

        using var request = CreateRequest(
            HttpMethod.Get,
            LibraryEndpoint,
            token);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithWrongSigningKey_ReturnsUnauthorized()
    {
        using var factory = new BackendFactory();
        using var client = CreateClient(factory);

        var token = CreateToken(
            key: "Another-Integration-Test-Key-12345678901234567890");

        using var request = CreateRequest(
            HttpMethod.Get,
            LibraryEndpoint,
            token);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithWrongIssuer_ReturnsUnauthorized()
    {
        using var factory = new BackendFactory();
        using var client = CreateClient(factory);

        var token = CreateToken(issuer: "UnknownIssuer");

        using var request = CreateRequest(
            HttpMethod.Get,
            LibraryEndpoint,
            token);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithWrongAudience_ReturnsUnauthorized()
    {
        using var factory = new BackendFactory();
        using var client = CreateClient(factory);

        var token = CreateToken(audience: "UnknownAudience");

        using var request = CreateRequest(
            HttpMethod.Get,
            LibraryEndpoint,
            token);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PresenceTicketEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new BackendFactory();
        using var client = CreateClient(factory);

        using var response = await client.PostAsync(
            TicketEndpoint,
            null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PresenceTicketEndpoint_WithValidJwt_ReturnsTicket()
    {
        using var factory = new BackendFactory();
        using var client = CreateClient(factory);

        var token = CreateToken();

        using var request = CreateRequest(
            HttpMethod.Post,
            TicketEndpoint,
            token);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ticketResponse =
            await response.Content.ReadFromJsonAsync<PresenceTicketResponse>();

        Assert.NotNull(ticketResponse);
        Assert.False(string.IsNullOrWhiteSpace(ticketResponse.Ticket));

        var ticket = new JwtSecurityTokenHandler()
            .ReadJwtToken(ticketResponse.Ticket);

        Assert.Equal("42", ticket.Subject);

        Assert.Contains(
            ticket.Claims,
            claim =>
                claim.Type == PresenceAuth.PurposeClaim &&
                claim.Value == PresenceAuth.PresencePurpose);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithPresenceTicket_ReturnsForbidden()
    {
        using var factory = new BackendFactory();
        using var client = CreateClient(factory);

        var token = CreateToken(
            additionalClaims: new[]
            {
                new Claim(
                    PresenceAuth.PurposeClaim,
                    PresenceAuth.PresencePurpose)
            });

        using var request = CreateRequest(
            HttpMethod.Get,
            LibraryEndpoint,
            token);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}