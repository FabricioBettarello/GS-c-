using System.Text.Json;
using System.Text.Json.Serialization;
using AlagaAlerta.Api.Erros;
using AlagaAlerta.Application;
using AlagaAlerta.Infrastructure;
using AlagaAlerta.Infrastructure.Data;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    })
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = RespostaValidacao.Criar);

builder.Services.AddExceptionHandler<TratadorDeErros>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AlagaAlerta API (.NET)",
        Version = "v1",
        Description = "Sensores ESP32 medem o nível da água em pontos que costumam alagar em São Paulo. " +
                      "A API classifica o risco, abre e encerra alertas e consolida indicadores por bairro. " +
                      "Global Solution FIAP 2026.2 - C# Software Development."
    });
    c.EnableAnnotations();
    c.SupportNonNullableReferenceTypes();
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddHealthChecks().AddDbContextCheck<AlagaAlertaDbContext>("banco");

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AlagaAlerta v1");
    c.DocumentTitle = "AlagaAlerta API";
});
app.MapScalarApiReference(o =>
{
    o.WithTitle("AlagaAlerta API")
     .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapHealthChecks("/health");
app.MapControllers();

if (app.Configuration.GetValue("Database:Inicializar", true))
{
    await DatabaseInitializer.InicializarAsync(app.Services, app.Configuration.GetValue("Database:Seed", false));
}

await app.RunAsync();

public partial class Program;
