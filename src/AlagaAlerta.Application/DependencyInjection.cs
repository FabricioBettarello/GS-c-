using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AlagaAlerta.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IBairroService, BairroService>();
        services.AddScoped<IPontoRiscoService, PontoRiscoService>();
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<ILeituraService, LeituraService>();
        services.AddScoped<IAlertaService, AlertaService>();
        services.AddScoped<IRelatoService, RelatoService>();
        services.AddScoped<IPainelService, PainelService>();

        return services;
    }
}
