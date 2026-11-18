extern alias Frontend;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BookHive.Tests.Helpers;

public sealed class AccountPageFactory(StubBackendHandler backend)
    : WebApplicationFactory<Frontend::Program>
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
