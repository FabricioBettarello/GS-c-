using AlagaAlerta.Domain;
using AlagaAlerta.Domain.Entities;
using AlagaAlerta.Domain.Exceptions;
using AlagaAlerta.Domain.ValueObjects;

namespace AlagaAlerta.Tests.Domain;

public class EntidadesTests
{
    private static PontoRisco NovoPonto() =>
        new("Praça da Bandeira", null, -23.5489, -46.6388, new LimitesAlagamento(10, 25), Guid.NewGuid());

    [Fact]
    public void Mensagem_critica_tem_nome_do_ponto_e_altura_arredondada()
    {
        var mensagem = Alerta.MontarMensagem(NivelAlerta.Critico, "Praça da Bandeira", 31.6);

        Assert.Contains("Praça da Bandeira", mensagem);
        Assert.Contains("32 cm", mensagem);
        Assert.Contains("Evite", mensagem);
    }

    [Fact]
    public void Alerta_emitido_comeca_ativo_e_pode_ser_encerrado_uma_vez()
    {
        var inicio = new DateTime(2026, 11, 10, 18, 0, 0, DateTimeKind.Utc);
        var alerta = Alerta.Emitir(NovoPonto(), NivelAlerta.Atencao, 12, inicio);

        Assert.True(alerta.Ativo);

        alerta.Encerrar(inicio.AddMinutes(40));

        Assert.False(alerta.Ativo);
        Assert.Equal(TimeSpan.FromMinutes(40), alerta.Duracao);
        Assert.Throws<DomainException>(() => alerta.Encerrar(inicio.AddHours(1)));
    }

    [Fact]
    public void Sensor_normaliza_o_codigo()
    {
        var sensor = new Sensor("  esp32-se-01 ", Guid.NewGuid());

        Assert.Equal("ESP32-SE-01", sensor.Codigo);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("ESP32 COM ESPACO")]
    [InlineData("ESP32_01")]
    public void Sensor_rejeita_codigo_invalido(string codigo) =>
        Assert.Throws<DomainException>(() => new Sensor(codigo, Guid.NewGuid()));

    [Fact]
    public void Sensor_inativo_nao_registra_leitura()
    {
        var sensor = new Sensor("ESP32-01", Guid.NewGuid());
        sensor.Desativar();

        Assert.Throws<DomainException>(() => sensor.RegistrarLeitura(10, null, DateTime.UtcNow));
    }

    [Fact]
    public void Leitura_fora_da_faixa_e_rejeitada()
    {
        var sensor = new Sensor("ESP32-01", Guid.NewGuid());

        Assert.Throws<DomainException>(() => sensor.RegistrarLeitura(-1, null, DateTime.UtcNow));
        Assert.Throws<DomainException>(() => sensor.RegistrarLeitura(10, 900, DateTime.UtcNow));
    }

    [Fact]
    public void Atualizar_ponto_valida_os_limites_resultantes()
    {
        var ponto = NovoPonto();

        Assert.Throws<DomainException>(() => ponto.Atualizar(nivelCriticoCm: 5));
        Assert.Equal(25, ponto.Limites.NivelCriticoCm);

        ponto.Atualizar(nivelCriticoCm: 35);
        Assert.Equal(35, ponto.Limites.NivelCriticoCm);
    }

    [Fact]
    public void Relato_so_pode_ser_moderado_uma_vez()
    {
        var relato = new Relato(Guid.NewGuid(), null, "Rua alagada", IntensidadeRelato.Alta, null, null);

        relato.Moderar(StatusRelato.Confirmado);

        Assert.Equal(StatusRelato.Confirmado, relato.Status);
        var erro = Assert.Throws<DomainException>(() => relato.Moderar(StatusRelato.Descartado));
        Assert.Contains("confirmado", erro.Message);
    }

    [Fact]
    public void Relato_nao_volta_para_aberto()
    {
        var relato = new Relato(Guid.NewGuid(), null, "Rua alagada", IntensidadeRelato.Baixa, null, null);

        Assert.Throws<DomainException>(() => relato.Moderar(StatusRelato.Aberto));
    }

    [Fact]
    public void Excecoes_de_dominio_mapeiam_para_status_http()
    {
        DomainException[] erros =
        [
            new DomainException("regra"),
            new NotFoundException("Bairro", Guid.Empty),
            new ConflictException("duplicado")
        ];

        Assert.Equal([422, 404, 409], erros.Select(e => e.StatusCode));
    }
}
