using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// Campos que el API agregó a FacturaCompraDTO para la compra de contado: la forma
/// de pago con que se cancela y la apertura de caja cuyo efectivo se reduce.
/// Se borra al regenerar el contrato NSwag.
/// </summary>
public partial class FacturaCompraDTO
{
    [JsonPropertyName("formaPagoContado")]
    public string? FormaPagoContado { get; set; }

    [JsonPropertyName("numAperturaContado")]
    public long NumAperturaContado { get; set; }
}
