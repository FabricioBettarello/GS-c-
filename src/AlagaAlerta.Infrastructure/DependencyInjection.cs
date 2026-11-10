using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Domain.Repositories;
using AlagaAlerta.Infrastructure.Data;
using AlagaAlerta.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlagaAlerta.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AlagaAlertaDbContext>((sp, options) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var provedor = configuration["Database:Provider"] ?? "Postgres";

            if (provedor.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase(configuration["Database:Name"] ?? "alagaalerta");
            }
            else
            {
                var conexao = configuration.GetConnectionString("AlagaAlerta")
                    ?? throw new InvalidOperationException("Connection string 'AlagaAlerta' não configurada.");
                options.UseNpgsql(conexao).UseSnakeCaseNamingConvention();
            }
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AlagaAlertaDbContext>());
        services.AddScoped<IBairroRepository, BairroRepository>();
        services.AddScoped<IPontoRiscoRepository, PontoRiscoRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<ILeituraRepository, LeituraRepository>();
        services.AddScoped<IAlertaRepository, AlertaRepository>();
        services.AddScoped<IRelatoRepository, RelatoRepository>();
        services.AddScoped<IPainelConsultas, PainelConsultas>();

        return services;
    }
}
