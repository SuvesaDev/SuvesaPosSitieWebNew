using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// Devoluciones parciales: cantidad de la línea que ya se devolvió en devoluciones
/// anteriores (no anuladas) de la misma venta. La pantalla solo permite devolver
/// <c>Cantidad - CantidadDevuelta</c>. Se borra al regenerar el contrato NSwag.
/// </summary>
public partial class FacturaDetallesDTO
{
    [JsonPropertyName("cantidadDevuelta")]
    public float CantidadDevuelta { get; set; }
}
