using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Contabilidad;

public sealed class PuertaSalidaDTO
{
    [JsonPropertyName("clave")] public string Clave { get; set; } = string.Empty;
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("cumplida")] public bool Cumplida { get; set; }
    [JsonPropertyName("detalle")] public string Detalle { get; set; } = string.Empty;
}

public sealed class FirmaUatDTO
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("firmadoPor")] public string FirmadoPor { get; set; } = string.Empty;
    [JsonPropertyName("fechaUtc")] public DateTime FechaUtc { get; set; }
    [JsonPropertyName("comentario")] public string? Comentario { get; set; }
    [JsonPropertyName("totalCasos")] public int TotalCasos { get; set; }
    [JsonPropertyName("vigente")] public bool Vigente { get; set; }
}

public sealed class PuestaEnMarchaDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("modo")] public string Modo { get; set; } = "Oficial";
    [JsonPropertyName("idSucursalPiloto")] public int? IdSucursalPiloto { get; set; }
    [JsonPropertyName("fechaPaseOficialUtc")] public DateTime? FechaPaseOficialUtc { get; set; }
    [JsonPropertyName("usuarioPaseOficial")] public string? UsuarioPaseOficial { get; set; }
    [JsonPropertyName("puertas")] public List<PuertaSalidaDTO> Puertas { get; set; } = new();
    [JsonPropertyName("puedePasarAOficial")] public bool PuedePasarAOficial { get; set; }
    [JsonPropertyName("casosUat")] public int CasosUat { get; set; }
    [JsonPropertyName("casosAprobados")] public int CasosAprobados { get; set; }
    [JsonPropertyName("casosRechazados")] public int CasosRechazados { get; set; }
    [JsonPropertyName("casosPendientes")] public int CasosPendientes { get; set; }
    [JsonPropertyName("firma")] public FirmaUatDTO? Firma { get; set; }
}

public sealed class CasoUatDTO
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("codigo")] public string Codigo { get; set; } = string.Empty;
    [JsonPropertyName("area")] public string Area { get; set; } = string.Empty;
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("estado")] public string Estado { get; set; } = string.Empty;
    [JsonPropertyName("comentario")] public string? Comentario { get; set; }
    [JsonPropertyName("usuario")] public string? Usuario { get; set; }
    [JsonPropertyName("fechaResultadoUtc")] public DateTime? FechaResultadoUtc { get; set; }
}

public sealed class IniciarSombraDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("idSucursalPiloto")] public int IdSucursalPiloto { get; set; }
}

public sealed class RegistrarResultadoUatDTO
{
    [JsonPropertyName("estado")] public string Estado { get; set; } = string.Empty;
    [JsonPropertyName("comentario")] public string? Comentario { get; set; }
}

public sealed class FirmarUatDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("comentario")] public string? Comentario { get; set; }
    [JsonPropertyName("contrasena")] public string? Contrasena { get; set; }
}

public sealed class PasarAOficialDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("contrasena")] public string? Contrasena { get; set; }
}

public sealed class ParaleloComandoDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("fechaCorte")] public DateOnly FechaCorte { get; set; }
    [JsonPropertyName("saldosReferencia")] public Dictionary<string, decimal> SaldosReferencia { get; set; } = new();
}

public sealed class ConfirmarSaldosInicialesDTO
{
    [JsonPropertyName("contrasena")] public string? Contrasena { get; set; }
}

public sealed class RechazarSaldosInicialesDTO
{
    [JsonPropertyName("motivo")] public string Motivo { get; set; } = string.Empty;
}
