using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>§7 lotes — lote a devolver (API MEJORA_LOTES §3.6). TEMPORAL hasta regen NSwag.</summary>
public partial class DevolucionVentaDetalleDTO
{
    [JsonPropertyName("idStockLote")]
    public long? IdStockLote { get; set; }
}

public partial class DevolucionCompraDetalleDTO
{
    [JsonPropertyName("idStockLote")]
    public long? IdStockLote { get; set; }
}
