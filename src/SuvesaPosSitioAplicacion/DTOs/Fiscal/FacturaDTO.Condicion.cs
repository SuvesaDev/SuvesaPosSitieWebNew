using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (el regen completo de contratos
// rompe ~50 proxies porque el API divergió). Al regenerar, el generado ya trae
// `esCredito` y este archivo se borra.
//
// Condición de venta elegida por el usuario en la pantalla de Facturación:
// true = crédito, false = contado. El Tipo de Factura solo determina si la
// condición está permitida; la Serie de Facturación ya no decide esto.

public partial class FacturaDTO
{
    [JsonPropertyName("esCredito")]
    public bool EsCredito { get; set; }
}
