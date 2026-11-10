using System.ComponentModel.DataAnnotations;
using AlagaAlerta.Domain;

namespace AlagaAlerta.Application.Dtos;

public sealed record CriarBairroRequest
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Nome { get; init; } = string.Empty;

    [Required]
    public Zona? Zona { get; init; }
}

public sealed record CriarPontoRiscoRequest
{
    [Required, StringLength(120, MinimumLength = 3)]
    public string Nome { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; init; }

    [Required, Range(-90, 90)]
    public double? Latitude { get; init; }

    [Required, Range(-180, 180)]
    public double? Longitude { get; init; }

    [Required, Range(0.1, 1000)]
    public double? NivelAtencaoCm { get; init; }

    [Required, Range(0.1, 1000)]
    public double? NivelCriticoCm { get; init; }

    [Required]
    public Guid? BairroId { get; init; }
}

public sealed record AtualizarPontoRiscoRequest
{
    [StringLength(120, MinimumLength = 3)]
    public string? Nome { get; init; }

    [StringLength(500)]
    public string? Descricao { get; init; }

    [Range(-90, 90)]
    public double? Latitude { get; init; }

    [Range(-180, 180)]
    public double? Longitude { get; init; }

    [Range(0.1, 1000)]
    public double? NivelAtencaoCm { get; init; }

    [Range(0.1, 1000)]
    public double? NivelCriticoCm { get; init; }

    public Guid? BairroId { get; init; }
}

public sealed record CriarSensorRequest
{
    [Required, RegularExpression("^[A-Za-z0-9-]{3,40}$", ErrorMessage = "Use letras, números e hífen (3 a 40 caracteres).")]
    public string Codigo { get; init; } = string.Empty;

    [Required]
    public Guid? PontoRiscoId { get; init; }

    public bool Ativo { get; init; } = true;
}

public sealed record AtualizarSensorRequest
{
    [Required]
    public bool? Ativo { get; init; }
}

public sealed record RegistrarLeituraRequest
{
    [Required, StringLength(40, MinimumLength = 3)]
    public string CodigoSensor { get; init; } = string.Empty;

    [Required, Range(0, 1000)]
    public double? NivelAguaCm { get; init; }

    [Range(0, 500)]
    public double? ChuvaMmHora { get; init; }

    public DateTimeOffset? RegistradaEm { get; init; }
}

public sealed record CriarRelatoRequest
{
    [Required]
    public Guid? BairroId { get; init; }

    public Guid? PontoRiscoId { get; init; }

    [Required, StringLength(500, MinimumLength = 5)]
    public string Descricao { get; init; } = string.Empty;

    [Required]
    public IntensidadeRelato? Intensidade { get; init; }

    [Range(-90, 90)]
    public double? Latitude { get; init; }

    [Range(-180, 180)]
    public double? Longitude { get; init; }
}

public sealed record ModerarRelatoRequest
{
    [Required]
    public StatusRelato? Status { get; init; }
}
