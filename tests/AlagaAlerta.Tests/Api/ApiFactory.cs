using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace AlagaAlerta.Tests.Api;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _banco = $"api-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = "InMemory",
                ["Database:Name"] = _banco,
                ["Database:Seed"] = "false"
            });
        });
    }
}
