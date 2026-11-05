using System.Text.Json.Serialization;
using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.DTOs.Contabilidad;

// Espejo de DTOs/Contabilidad/IntegridadContableDTOs.cs del API (controles de integridad contable).
// Cliente y DTOs a mano hasta regenerar los contratos NSwag contra el API desplegado.

public sealed class DocumentoSinEventoDTO
{
    [JsonPropertyName("tipo")] public string Tipo { get; set; } = "";
    [JsonPropertyName("idDocumento")] public long IdDocumento { get; set; }
    [JsonPropertyName("numero")] public string Numero { get; set; } = "";
    [JsonPropertyName("fecha")] public DateTime Fecha { get; set; }
    [JsonPropertyName("total")] public decimal Total { get; set; }
    [JsonPropertyName("idSucursal")] public int? IdSucursal { get; set; }
    [JsonPropertyName("motivo")] public string Motivo { get; set; } = "";
}

public sealed class DocumentosSinEventoDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("desde")] public DateOnly Desde { get; set; }
    [JsonPropertyName("hasta")] public DateOnly Hasta { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("documentos")] public List<DocumentoSinEventoDTO> Documentos { get; set; } = new();
}

public sealed class RangoIntegridadContableDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("desde")] public DateOnly? Desde { get; set; }
    [JsonPropertyName("hasta")] public DateOnly? Hasta { get; set; }
}

public sealed class RegularizacionDocumentosDTO
{
    [JsonPropertyName("regularizados")] public int Regularizados { get; set; }
    [JsonPropertyName("sinCambio")] public int SinCambio { get; set; }
    [JsonPropertyName("errores")] public List<string> Errores { get; set; } = new();
}

public sealed class ProblemaPlantillaDTO
{
    [JsonPropertyName("tipoEvento")] public string TipoEvento { get; set; } = "";
    /// <summary>Error (los eventos fallan) o Aviso.</summary>
    [JsonPropertyName("gravedad")] public string Gravedad { get; set; } = "";
    [JsonPropertyName("detalle")] public string Detalle { get; set; } = "";
}

public sealed class DiagnosticoPlantillasDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("fecha")] public DateOnly Fecha { get; set; }
    [JsonPropertyName("eventosEnErrorConfiguracion")] public int EventosEnErrorConfiguracion { get; set; }
    [JsonPropertyName("sano")] public bool Sano { get; set; }
    [JsonPropertyName("problemas")] public List<ProblemaPlantillaDTO> Problemas { get; set; } = new();
}

public sealed class ReencolarErroresDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("tipoEvento")] public string? TipoEvento { get; set; }
}

public sealed class ResultadoReencoladoDTO
{
    [JsonPropertyName("reencolados")] public int Reencolados { get; set; }
}

/// <summary>Envoltorio <c>ResponseGeneric&lt;T&gt;</c> del API para los clientes escritos a mano.</summary>
public sealed class RespuestaIntegridad<T> where T : class
{
    [JsonPropertyName("status")] public ResponseStatus Status { get; set; }
    [JsonPropertyName("currentException")] public string? CurrentException { get; set; }
    [JsonPropertyName("validationErrors")] public ICollection<string>? ValidationErrors { get; set; }
    [JsonPropertyName("responses")] public T? Responses { get; set; }
}
