using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (igual que FacturaDTO.Condicion.cs).
// Al regenerar los contratos el generado ya trae `pagos` y este archivo se borra.
//
// Formas de pago recibidas al emitir una factura de contado. La suma de los montos
// debe cubrir el Total. Vacío/ignorado para crédito, preventa y consignación.

public partial class FacturaDTO
{
    [JsonPropertyName("pagos")]
    public List<PagoFacturaDTO> Pagos { get; set; } = new();
}

public sealed class PagoFacturaDTO
{
    [JsonPropertyName("formaPago")]
    public string FormaPago { get; set; } = string.Empty;

    [JsonPropertyName("monto")]
    public double Monto { get; set; }

    [JsonPropertyName("referencia")]
    public string? Referencia { get; set; }
}
