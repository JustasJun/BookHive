using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BookHive.Tests.Integration;

public class AccountAuthenticationTests
{
    [Fact]
    public async Task Register_WhenBackendAcceptsUser_SignsInAndRedirectsToHome()
    {
        var backend = new StubBackendHandler(
            ("POST", "/api/account/register", HttpStatusCode.OK,
                """{"id":42,"username":"reader","email":"reader@example.com"}"""));
        using var factory = new AccountPageFactory(backend);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsync(
            "/Account/Register",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["username"] = "reader",
                ["email"] = "reader@example.com",
                ["password"] = "SafePassword123!"
            }));
        string homePage = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Your social network for book lovers.", homePage);

        RecordedRequest request = Assert.Single(backend.Requests);
        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/account/register", request.PathAndQuery);
        using JsonDocument body = JsonDocument.Parse(request.Body);
        Assert.Equal("reader", body.RootElement.GetProperty("username").GetString());
        Assert.Equal("reader@example.com", body.RootElement.GetProperty("email").GetString());
        Assert.Equal("SafePassword123!", body.RootElement.GetProperty("password").GetString());
    }

    [Fact]
    public async Task Register_WhenBackendRejectsUser_ShowsBackendError()
    {
        var backend = new StubBackendHandler(
            ("POST", "/api/account/register", HttpStatusCode.BadRequest,
                """{"message":"Email is already in use."}"""));
        using var factory = new AccountPageFactory(backend);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsync(
            "/Account/Register",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["username"] = "reader",
                ["email"] = "reader@example.com",
                ["password"] = "SafePassword123!"
            }));
        string page = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Email is already in use.", page);
        Assert.DoesNotContain(backend.Requests, request => request.PathAndQuery == "/api/goals?userId=42");
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_SignsInAndRedirectsToHome()
    {
        var backend = new StubBackendHandler(
            ("POST", "/api/account/login", HttpStatusCode.OK,
                """{"id":42,"username":"reader","email":"reader@example.com"}"""),
            ("GET", "/api/goals?userId=42", HttpStatusCode.NotFound, ""));
        using var factory = new AccountPageFactory(backend);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["loginInput"] = "reader@example.com",
                ["password"] = "SafePassword123!"
            }));
        string homePage = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Your social network for book lovers.", homePage);

        RecordedRequest loginRequest = Assert.Single(
            backend.Requests,
            request => request.PathAndQuery == "/api/account/login");
        using JsonDocument body = JsonDocument.Parse(loginRequest.Body);
        Assert.Equal("reader@example.com", body.RootElement.GetProperty("loginInput").GetString());
        Assert.Equal("SafePassword123!", body.RootElement.GetProperty("password").GetString());
        Assert.Contains(backend.Requests, request => request.PathAndQuery == "/api/goals?userId=42");
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ShowsErrorAndDoesNotSignIn()
    {
        var backend = new StubBackendHandler(
            ("POST", "/api/account/login", HttpStatusCode.Unauthorized, ""));
        using var factory = new AccountPageFactory(backend);
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["loginInput"] = "reader@example.com",
                ["password"] = "wrong-password"
            }));
        string page = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Invalid login credentials. Please try again.", page);
        Assert.DoesNotContain("Your social network for book lovers.", page);
        Assert.Single(backend.Requests);
    }

    private sealed class AccountPageFactory(StubBackendHandler backend) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddHttpClient("BackendApi")
                    .ConfigurePrimaryHttpMessageHandler(() => backend);
            });
        }
    }

    private sealed class StubBackendHandler(
        params (string Method, string PathAndQuery, HttpStatusCode StatusCode, string Body)[] responses)
        : HttpMessageHandler
    {
        private readonly IReadOnlyDictionary<(string Method, string PathAndQuery), (HttpStatusCode StatusCode, string Body)> _responses =
            responses.ToDictionary(
                response => (response.Method, response.PathAndQuery),
                response => (response.StatusCode, response.Body));

        public ConcurrentQueue<RecordedRequest> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string method = request.Method.Method;
            string pathAndQuery = request.RequestUri?.PathAndQuery
                ?? throw new InvalidOperationException("Backend request has no URI.");
            string body = request.Content == null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            Requests.Enqueue(new RecordedRequest(method, pathAndQuery, body));

            if (!_responses.TryGetValue((method, pathAndQuery), out var response))
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return new HttpResponseMessage(response.StatusCode)
            {
                Content = new StringContent(response.Body, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed record RecordedRequest(string Method, string PathAndQuery, string Body);
}
