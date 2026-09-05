using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (mismo motivo que FacturaDTO.Condicion.cs).
// Al regenerar, el generado ya trae `tipoCambio` y este archivo se borra.
//
// Tipo de cambio CRC/USD consultado al facturar en dólares. Null o 1 si la
// venta es en colones.

public partial class FacturaDTO
{
    [JsonPropertyName("tipoCambio")]
    public double? TipoCambio { get; set; }
}
