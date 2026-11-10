using AlagaAlerta.Domain;
using AlagaAlerta.Domain.Exceptions;
using AlagaAlerta.Domain.ValueObjects;

namespace AlagaAlerta.Tests.Domain;

public class LimitesAlagamentoTests
{
    private readonly LimitesAlagamento _limites = new(15, 30);

    [Theory]
    [InlineData(0)]
    [InlineData(14.9)]
    public void Abaixo_da_atencao_e_normal(double nivel) =>
        Assert.Equal(NivelRisco.Normal, _limites.Classificar(nivel));

    [Theory]
    [InlineData(15)]
    [InlineData(29.9)]
    public void A_partir_da_atencao_e_atencao(double nivel) =>
        Assert.Equal(NivelRisco.Atencao, _limites.Classificar(nivel));

    [Theory]
    [InlineData(30)]
    [InlineData(120)]
    public void A_partir_do_critico_e_critico(double nivel) =>
        Assert.Equal(NivelRisco.Critico, _limites.Classificar(nivel));

    [Theory]
    [InlineData(30, 30)]
    [InlineData(30, 10)]
    [InlineData(0, 10)]
    [InlineData(-5, 10)]
    public void Rejeita_limites_incoerentes(double atencao, double critico) =>
        Assert.Throws<DomainException>(() => new LimitesAlagamento(atencao, critico));

    [Fact]
    public void Com_mantem_o_valor_que_nao_foi_informado()
    {
        var novos = _limites.Com(null, 40);

        Assert.Equal(15, novos.NivelAtencaoCm);
        Assert.Equal(40, novos.NivelCriticoCm);
    }

    [Fact]
    public void Limites_com_mesmos_valores_sao_iguais() =>
        Assert.Equal(new LimitesAlagamento(15, 30), _limites);
}
