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
    [JsonPropertyName("fechaPuestaEnMarcha")] public DateOnly? FechaPuestaEnMarcha { get; set; }
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

public sealed class DuplicarVersionPlantillaDTO
{
    [JsonPropertyName("efectivaDesde")] public DateOnly EfectivaDesde { get; set; }
    [JsonPropertyName("efectivaHasta")] public DateOnly? EfectivaHasta { get; set; }
}

public sealed class FijarPuestaEnMarchaDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("fecha")] public DateOnly? Fecha { get; set; }
}

public sealed class ForzarCompraDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("motivo")] public string Motivo { get; set; } = "";
}

public sealed class ResultadoReencoladoDTO
{
    [JsonPropertyName("reencolados")] public int Reencolados { get; set; }
}

public sealed class LineaPropuestaProrrataDTO
{
    [JsonPropertyName("orden")] public int Orden { get; set; }
    [JsonPropertyName("tipoMovimiento")] public string TipoMovimiento { get; set; } = "";
    [JsonPropertyName("codigoCuenta")] public string CodigoCuenta { get; set; } = "";
    [JsonPropertyName("cuenta")] public string Cuenta { get; set; } = "";
    [JsonPropertyName("monto")] public decimal Monto { get; set; }
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = "";
}

/// <summary>La póliza que el sistema propone para trasladar el IVA sujeto a proporcionalidad (condición 05) a crédito y a gasto.</summary>
public sealed class PropuestaProrrataIvaDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("ejercicio")] public int Ejercicio { get; set; }
    /// <summary>1 a 12: traslado mensual con el porcentaje provisional. 13: regularización anual con el definitivo.</summary>
    [JsonPropertyName("mes")] public int Mes { get; set; }
    [JsonPropertyName("tipo")] public string Tipo { get; set; } = "";
    [JsonPropertyName("fechaContable")] public DateOnly FechaContable { get; set; }
    [JsonPropertyName("ivaSujetoProrrata")] public decimal IvaSujetoProrrata { get; set; }
    [JsonPropertyName("porcentajeAplicado")] public decimal PorcentajeAplicado { get; set; }
    [JsonPropertyName("origenPorcentaje")] public string OrigenPorcentaje { get; set; } = "";
    [JsonPropertyName("creditoAcreditable")] public decimal CreditoAcreditable { get; set; }
    [JsonPropertyName("gastoNoAcreditable")] public decimal GastoNoAcreditable { get; set; }
    [JsonPropertyName("periodoCompleto")] public bool PeriodoCompleto { get; set; }
    [JsonPropertyName("idSolicitud")] public long? IdSolicitud { get; set; }
    [JsonPropertyName("estadoSolicitud")] public string? EstadoSolicitud { get; set; }
    [JsonPropertyName("lineas")] public List<LineaPropuestaProrrataDTO> Lineas { get; set; } = new();
    [JsonPropertyName("advertencias")] public List<string> Advertencias { get; set; } = new();
}

public sealed class SolicitarProrrataIvaDTO
{
    [JsonPropertyName("idLibroContable")] public long IdLibroContable { get; set; }
    [JsonPropertyName("ejercicio")] public int Ejercicio { get; set; }
    [JsonPropertyName("mes")] public int Mes { get; set; }
}

/// <summary>Envoltorio <c>ResponseGeneric&lt;T&gt;</c> del API para los clientes escritos a mano.</summary>
public sealed class RespuestaIntegridad<T>
{
    [JsonPropertyName("status")] public ResponseStatus Status { get; set; }
    [JsonPropertyName("currentException")] public string? CurrentException { get; set; }
    [JsonPropertyName("validationErrors")] public ICollection<string>? ValidationErrors { get; set; }
    [JsonPropertyName("responses")] public T? Responses { get; set; }
}
