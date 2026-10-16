using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MyWeather.WebApi.Tests.Infrastructure;

// Hosts the real endpoints and validation filters with MediatR replaced by a FakeSender. No database is used.
public class FakeSenderApiFactory : WebApplicationFactory<Program>
{
    public FakeSender Sender { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        TestEnvironment.UseIsolatedSettings(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ISender>();
            services.AddSingleton<ISender>(Sender);
        });
    }
}
