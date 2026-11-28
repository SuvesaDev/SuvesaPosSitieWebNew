using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (mismo patrón que CotizacionesDTO.IdClienteSucursal.cs). El API ya recibe `dejarComoSaldoAFavor`;
// al regenerar los contratos este archivo se borra.
//
// Cambio de producto en una factura de contado: todo el monto de la nota queda como saldo a favor del cliente (no sale de caja).

public partial class DevolucionVentaDTO
{
    [JsonPropertyName("dejarComoSaldoAFavor")]
    public bool DejarComoSaldoAFavor { get; set; }
}
