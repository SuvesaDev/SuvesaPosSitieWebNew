using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano a los DTO generados (mismo patrón que CotizacionesDTO.IdClienteSucursal.cs). El API ya recibe y
// devuelve `reglaCreditoVenta`; al regenerar los contratos este archivo se borra.
//
// CON_DERECHO | SIN_DERECHO | null (según la tarifa del artículo): si las ventas dan derecho a crédito fiscal (prorrata del IVA).

public partial class ClienteDTO
{
    [JsonPropertyName("reglaCreditoVenta")]
    public string? ReglaCreditoVenta { get; set; }
}

public partial class FiltranClienteDTO
{
    [JsonPropertyName("reglaCreditoVenta")]
    public string? ReglaCreditoVenta { get; set; }
}

public partial class CartaExoneracionDTO
{
    [JsonPropertyName("reglaCreditoVenta")]
    public string? ReglaCreditoVenta { get; set; }
}
