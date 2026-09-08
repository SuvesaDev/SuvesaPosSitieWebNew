using System.Text.Json.Serialization;
using SuvesaPosSitioAplicacion.DTOs.Lotes;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// §5 lotes — reparto de una línea de venta en varios lotes (API MEJORA_LOTES §3.4).
/// TEMPORAL: al regenerar contratos NSwag el generado ya trae este campo.
/// </summary>
public partial class FacturaDetallesDTO
{
    [JsonPropertyName("lotes")]
    public System.Collections.Generic.List<LoteConsumoVenta>? Lotes { get; set; }
}
