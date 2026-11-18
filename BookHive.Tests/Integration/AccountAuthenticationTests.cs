using BookHive.Tests.Helpers;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BookHive.Tests.Integration;

public class AccountAuthenticationTests
{
    private const string AuthCookieName = ".AspNetCore.Cookies";
    private const string HomeMarker = "Your social network for book lovers.";

    [Fact]
    public async Task Register_WhenBackendAcceptsUser_IssuesAuthCookieAndRedirectsToHome()
    {
        var backend = new StubBackendHandler(
            ("POST", "/api/account/register", HttpStatusCode.OK,
                """{"id":42,"username":"reader","email":"reader@example.com","token":"test-jwt-token"}"""));
        using var factory = new AccountPageFactory(backend);
        using HttpClient client = CreateClient(factory);

        using HttpResponseMessage response = await client.PostAsync(
            "/Account/Register",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["username"] = "reader",
                ["email"] = "reader@example.com",
                ["password"] = "SafePassword123!"
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Home", response.Headers.Location?.OriginalString);
        Assert.True(HasAuthCookie(response), "Authentication cookie was not issued.");

        // The issued cookie must actually grant access to a protected page.
        await AssertHomeIsAccessibleAsync(client);

        RecordedRequest request = Assert.Single(backend.Requests);
        Assert.Equal("POST", request.Method);
        Assert.Equal("/api/account/register", request.PathAndQuery);
        using JsonDocument body = JsonDocument.Parse(request.Body);
        Assert.Equal("reader", body.RootElement.GetProperty("username").GetString());
        Assert.Equal("reader@example.com", body.RootElement.GetProperty("email").GetString());
        Assert.Equal("SafePassword123!", body.RootElement.GetProperty("password").GetString());
    }

    [Fact]
    public async Task Register_WhenUserAlreadyExists_ShowsConflictMessageAndDoesNotSignIn()
    {
        var backend = new StubBackendHandler(
            ("POST", "/api/account/register", HttpStatusCode.Conflict,
                """{"message":"Username is already taken."}"""));
        using var factory = new AccountPageFactory(backend);
        using HttpClient client = CreateClient(factory);

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
        Assert.Contains("Username is already taken.", page);
        Assert.False(HasAuthCookie(response), "Authentication cookie must not be issued.");
        await AssertHomeIsProtectedAsync(client);
    }

    [Fact]
    public async Task Register_WhenBackendRejectsUser_ShowsBackendErrorAndDoesNotSignIn()
    {
        var backend = new StubBackendHandler(
            ("POST", "/api/account/register", HttpStatusCode.BadRequest,
                """{"message":"Email is already in use."}"""));
        using var factory = new AccountPageFactory(backend);
        using HttpClient client = CreateClient(factory);

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
        Assert.DoesNotContain(HomeMarker, page);
        Assert.False(HasAuthCookie(response), "Authentication cookie must not be issued.");
        await AssertHomeIsProtectedAsync(client);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_IssuesAuthCookieAndRedirectsToHome()
    {
        var backend = new StubBackendHandler(
            ("POST", "/api/account/login", HttpStatusCode.OK,
                """{"id":42,"username":"reader","email":"reader@example.com","token":"test-jwt-token"}"""),
            ("GET", "/api/goals", HttpStatusCode.NotFound, ""));
        using var factory = new AccountPageFactory(backend);
        using HttpClient client = CreateClient(factory);

        using HttpResponseMessage response = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["loginInput"] = "reader@example.com",
                ["password"] = "SafePassword123!"
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Home", response.Headers.Location?.OriginalString);
        Assert.True(HasAuthCookie(response), "Authentication cookie was not issued.");

        await AssertHomeIsAccessibleAsync(client);

        RecordedRequest loginRequest = Assert.Single(
            backend.Requests,
            request => request.PathAndQuery == "/api/account/login");
        using JsonDocument body = JsonDocument.Parse(loginRequest.Body);
        Assert.Equal("reader@example.com", body.RootElement.GetProperty("loginInput").GetString());
        Assert.Equal("SafePassword123!", body.RootElement.GetProperty("password").GetString());
        RecordedRequest goalRequest = Assert.Single(
            backend.Requests,
            request => request.PathAndQuery == "/api/goals");

        Assert.Equal("Bearer test-jwt-token", goalRequest.Authorization);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ShowsErrorAndDoesNotSignIn()
    {
        var backend = new StubBackendHandler(
            ("POST", "/api/account/login", HttpStatusCode.Unauthorized, ""));
        using var factory = new AccountPageFactory(backend);
        using HttpClient client = CreateClient(factory);

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
        Assert.DoesNotContain(HomeMarker, page);
        Assert.False(HasAuthCookie(response), "Authentication cookie must not be issued.");
        await AssertHomeIsProtectedAsync(client);
        Assert.Single(backend.Requests);
    }

    [Fact]
    public async Task Logout_WhenSignedIn_RemovesAccessToProtectedPages()
    {
        var backend = new StubBackendHandler(
            ("POST", "/api/account/login", HttpStatusCode.OK,
                """{"id":42,"username":"reader","email":"reader@example.com","token":"test-jwt-token"}"""),
            ("GET", "/api/goals", HttpStatusCode.NotFound, ""));
        using var factory = new AccountPageFactory(backend);
        using HttpClient client = CreateClient(factory);

        using HttpResponseMessage loginResponse = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["loginInput"] = "reader@example.com",
                ["password"] = "SafePassword123!"
            }));
        Assert.True(HasAuthCookie(loginResponse));
        await AssertHomeIsAccessibleAsync(client);

        using HttpResponseMessage logoutResponse = await client.PostAsync(
            "/Account/Logout",
            new FormUrlEncodedContent(new Dictionary<string, string>()));

        Assert.Equal(HttpStatusCode.Redirect, logoutResponse.StatusCode);
        Assert.False(HasAuthCookie(logoutResponse), "Authentication cookie must be cleared.");
        await AssertHomeIsProtectedAsync(client);
    }

    [Fact]
    public async Task Goals_WhenSignedIn_SendsBearerTokenToBackend()
    {
        var backend = new StubBackendHandler(
            ("POST", "/api/account/login", HttpStatusCode.OK,
                """{"id":42,"username":"reader","email":"reader@example.com","token":"test-jwt-token"}"""),
            ("GET", "/api/goals", HttpStatusCode.NotFound, ""),
            ("GET", "/api/goals/progress", HttpStatusCode.NotFound, ""),
            ("GET", "/api/goals/history", HttpStatusCode.NotFound, ""));

        using var factory = new AccountPageFactory(backend);
        using HttpClient client = CreateClient(factory);

        // First, sign in and receive the authentication cookie.
        using HttpResponseMessage loginResponse = await client.PostAsync(
            "/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["loginInput"] = "reader@example.com",
                ["password"] = "SafePassword123!"
            }));

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
        Assert.True(HasAuthCookie(loginResponse));

        // This is a separate request, so the cookie is now available.
        using HttpResponseMessage goalsResponse = await client.GetAsync("/Goals");

        Assert.Equal(HttpStatusCode.OK, goalsResponse.StatusCode);

        // Both Backend requests should receive the stored JWT automatically.
        RecordedRequest progressRequest = Assert.Single(
            backend.Requests,
            request => request.PathAndQuery == "/api/goals/progress");

        RecordedRequest historyRequest = Assert.Single(
            backend.Requests,
            request => request.PathAndQuery == "/api/goals/history");

        Assert.Equal("Bearer test-jwt-token", progressRequest.Authorization);
        Assert.Equal("Bearer test-jwt-token", historyRequest.Authorization);
    }

    // Redirects are not followed, so the tests can inspect the redirect and Set-Cookie headers.
    // The client still stores cookies and sends them on subsequent requests.
    private static HttpClient CreateClient(AccountPageFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static bool HasAuthCookie(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values)
        && values.Any(value =>
            value.StartsWith(AuthCookieName + "=", StringComparison.Ordinal)
            && !value.StartsWith(AuthCookieName + "=;", StringComparison.Ordinal));

    private static async Task AssertHomeIsAccessibleAsync(HttpClient client)
    {
        using HttpResponseMessage response = await client.GetAsync("/Home");
        string page = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(HomeMarker, page);
    }

    private static async Task AssertHomeIsProtectedAsync(HttpClient client)
    {
        using HttpResponseMessage response = await client.GetAsync("/Home");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString);
    }
}
