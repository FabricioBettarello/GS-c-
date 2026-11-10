using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

var codigoSensor = args.ElementAtOrDefault(0) ?? "ESP32-POMPEIA-01";
var baseUrl = Environment.GetEnvironmentVariable("API_URL") ?? "http://localhost:5080/api/v1/";
var intervalo = TimeSpan.FromMilliseconds(int.TryParse(Environment.GetEnvironmentVariable("INTERVALO_MS"), out var ms) ? ms : 2000);

(double NivelAguaCm, double ChuvaMmHora)[] tempestade =
[
    (3, 5), (8, 18), (14, 30), (19, 42), (26, 55), (33, 60), (38, 48),
    (31, 30), (22, 15), (12, 6), (5, 0)
];

var json = new JsonSerializerOptions(JsonSerializerDefaults.Web);
json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));

using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
using var cancelamento = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancelamento.Cancel();
};

Console.WriteLine($"Simulando {codigoSensor} -> {baseUrl}leituras");
Console.WriteLine();

try
{
    foreach (var (nivel, chuva) in tempestade)
    {
        var resposta = await http.PostAsJsonAsync(
            "leituras",
            new { codigoSensor, nivelAguaCm = nivel, chuvaMmHora = chuva },
            json,
            cancelamento.Token);

        if (!resposta.IsSuccessStatusCode)
        {
            Console.Error.WriteLine($"Erro {(int)resposta.StatusCode}: {await resposta.Content.ReadAsStringAsync(cancelamento.Token)}");
            return 1;
        }

        var resultado = await resposta.Content.ReadFromJsonAsync<Resultado>(json, cancelamento.Token);
        Console.WriteLine($"água {nivel,3} cm | chuva {chuva,2} mm/h | {resultado!.Nivel,-8} | {resultado.Acao}");
        if (resultado.Acao == "ENCERRADO")
        {
            Console.WriteLine("   -> alerta encerrado, a água baixou");
        }
        else if (resultado.Alerta is not null && resultado.Acao != "MANTIDO")
        {
            Console.WriteLine($"   -> {resultado.Alerta.Mensagem}");
        }

        await Task.Delay(intervalo, cancelamento.Token);
    }
}
catch (HttpRequestException e)
{
    Console.Error.WriteLine($"Falha ao chamar a API em {baseUrl}: {e.Message}");
    return 1;
}
catch (OperationCanceledException)
{
    Console.WriteLine("Simulação interrompida.");
}

return 0;

internal sealed record Alerta(string Mensagem);

internal sealed record Resultado(string Nivel, string Acao, Alerta? Alerta);
