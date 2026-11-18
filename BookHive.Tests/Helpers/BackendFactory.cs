using BookHive.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BookHive.Tests.Helpers;

public sealed class BackendFactory : WebApplicationFactory<Program>
{
    public const string JwtKey = "BookHive-Integration-Test-Key-12345678901234567890";
    public const string JwtIssuer = "BookHive.Tests";
    public const string JwtAudience = "BookHive.Tests.Client";

    private readonly HttpMessageHandler? _openLibrary;

    public BackendFactory(HttpMessageHandler? openLibrary = null)
    {
        _openLibrary = openLibrary;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Jwt:Issuer", JwtIssuer);
        builder.UseSetting("Jwt:Audience", JwtAudience);

        if (_openLibrary != null)
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddHttpClient<IOpenLibraryClient, OpenLibraryClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _openLibrary);
            });
        }
    }
}