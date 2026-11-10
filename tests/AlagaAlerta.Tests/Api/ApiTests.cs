using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AlagaAlerta.Tests.Api;

public class ApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory = new();
    private HttpClient _http = null!;

    public Task InitializeAsync()
    {
        _http = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _http.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task<JsonNode> PostAsync(string url, object corpo, HttpStatusCode esperado = HttpStatusCode.Created)
    {
        var resposta = await _http.PostAsJsonAsync(url, corpo);
        var texto = await resposta.Content.ReadAsStringAsync();
        Assert.True(resposta.StatusCode == esperado, $"{url}: esperado {esperado}, veio {resposta.StatusCode}. {texto}");
        return JsonNode.Parse(texto)!;
    }

    private async Task<JsonNode> GetAsync(string url)
    {
        var resposta = await _http.GetAsync(url);
        resposta.EnsureSuccessStatusCode();
        return JsonNode.Parse(await resposta.Content.ReadAsStringAsync())!;
    }

    private async Task<(string BairroId, string PontoId)> PrepararPontoAsync()
    {
        var bairro = await PostAsync("/api/v1/bairros", new { nome = "Pompeia", zona = "OESTE" });
        var ponto = await PostAsync("/api/v1/pontos-risco", new
        {
            nome = "Av. Pompeia x R. Turiassu",
            latitude = -23.5305,
            longitude = -46.6835,
            nivelAtencaoCm = 15,
            nivelCriticoCm = 30,
            bairroId = bairro["id"]!.GetValue<string>()
        });
        await PostAsync("/api/v1/sensores", new { codigo = "esp32-pompeia-01", pontoRiscoId = ponto["id"]!.GetValue<string>() });
        return (bairro["id"]!.GetValue<string>(), ponto["id"]!.GetValue<string>());
    }

    [Fact]
    public async Task Health_responde_ok()
    {
        var resposta = await _http.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Cria_e_lista_bairros()
    {
        var criado = await PostAsync("/api/v1/bairros", new { nome = "Sé", zona = "CENTRO" });

        Assert.Equal("Sé", criado["nome"]!.GetValue<string>());
        Assert.Equal("CENTRO", criado["zona"]!.GetValue<string>());

        var lista = await GetAsync("/api/v1/bairros");
        Assert.Single(lista.AsArray());
    }

    [Fact]
    public async Task Corpo_invalido_retorna_400_com_os_campos()
    {
        var erro = await PostAsync("/api/v1/bairros", new { nome = "X" }, HttpStatusCode.BadRequest);

        Assert.Equal("Dados inválidos", erro["erro"]!.GetValue<string>());
        var campos = erro["detalhes"]!.AsArray().Select(d => d!["campo"]!.GetValue<string>()).ToList();
        Assert.Contains("nome", campos);
        Assert.Contains("zona", campos);
    }

    [Fact]
    public async Task Bairro_repetido_retorna_409()
    {
        await PostAsync("/api/v1/bairros", new { nome = "Sé", zona = "CENTRO" });
        await PostAsync("/api/v1/bairros", new { nome = "Sé", zona = "CENTRO" }, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Limites_incoerentes_retornam_422()
    {
        var bairro = await PostAsync("/api/v1/bairros", new { nome = "Lapa", zona = "OESTE" });

        await PostAsync("/api/v1/pontos-risco", new
        {
            nome = "Ponto ruim",
            latitude = 0,
            longitude = 0,
            nivelAtencaoCm = 30,
            nivelCriticoCm = 10,
            bairroId = bairro["id"]!.GetValue<string>()
        }, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Id_inexistente_retorna_404()
    {
        var resposta = await _http.GetAsync($"/api/v1/bairros/{Guid.Empty}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Leitura_critica_gera_alerta_e_agua_baixando_encerra()
    {
        var (bairroId, pontoId) = await PrepararPontoAsync();

        var leitura = await PostAsync("/api/v1/leituras", new { codigoSensor = "ESP32-POMPEIA-01", nivelAguaCm = 34, chuvaMmHora = 45 });
        Assert.Equal("CRITICO", leitura["nivel"]!.GetValue<string>());
        Assert.Equal("CRIADO", leitura["acao"]!.GetValue<string>());

        var alertas = await GetAsync($"/api/v1/alertas?ativos=true&bairroId={bairroId}");
        Assert.Single(alertas.AsArray());
        Assert.Contains("Av. Pompeia", alertas[0]!["mensagem"]!.GetValue<string>());

        var status = await GetAsync($"/api/v1/pontos-risco/{pontoId}/status");
        Assert.Equal("CRITICO", status["nivelAtual"]!.GetValue<string>());

        var baixou = await PostAsync("/api/v1/leituras", new { codigoSensor = "ESP32-POMPEIA-01", nivelAguaCm = 2 });
        Assert.Equal("ENCERRADO", baixou["acao"]!.GetValue<string>());
        Assert.Empty((await GetAsync("/api/v1/alertas?ativos=true")).AsArray());

        var painel = await GetAsync("/api/v1/painel");
        Assert.Equal(1, painel["bairros"]![0]!["alertasEmitidos"]!.GetValue<int>());
    }

    [Fact]
    public async Task Cidadao_relata_e_relato_e_confirmado()
    {
        var (bairroId, pontoId) = await PrepararPontoAsync();

        var relato = await PostAsync("/api/v1/relatos", new
        {
            bairroId,
            pontoRiscoId = pontoId,
            descricao = "Água na altura do joelho",
            intensidade = "ALTA"
        });
        Assert.Equal("ABERTO", relato["status"]!.GetValue<string>());

        var resposta = await _http.PatchAsJsonAsync($"/api/v1/relatos/{relato["id"]}/status", new { status = "CONFIRMADO" });
        var moderado = JsonNode.Parse(await resposta.Content.ReadAsStringAsync())!;
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("CONFIRMADO", moderado["status"]!.GetValue<string>());
    }

    [Fact]
    public async Task Documentacao_openapi_e_gerada_automaticamente()
    {
        var documento = await GetAsync("/swagger/v1/swagger.json");

        Assert.StartsWith("3.", documento["openapi"]!.GetValue<string>());
        var caminhos = documento["paths"]!.AsObject().Select(p => p.Key).ToList();
        Assert.Contains("/api/v1/bairros", caminhos);
        Assert.Contains("/api/v1/leituras", caminhos);
        Assert.Contains("/api/v1/pontos-risco/{id}/status", caminhos);
        Assert.Contains("/api/v1/painel", caminhos);

        var scalar = await _http.GetAsync("/scalar/v1");
        Assert.Equal(HttpStatusCode.OK, scalar.StatusCode);
    }
}
