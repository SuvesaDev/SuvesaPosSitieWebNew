using System.Text.Json.Serialization;
using SuvesaPosSitioAplicacion.DTOs.Lotes;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// §6 lotes — uno o varios lotes que ingresan con una línea de compra
/// (API MEJORA_LOTES §3.5). TEMPORAL hasta regenerar contratos NSwag.
/// </summary>
public partial class FacturaCompraDetalleDTO
{
    [JsonPropertyName("lotes")]
    public System.Collections.Generic.List<LoteIngresoCompra>? Lotes { get; set; }
}
