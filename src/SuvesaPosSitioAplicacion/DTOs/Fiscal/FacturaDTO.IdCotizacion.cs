using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (mismo patrón que FacturaDTO.IdSerie.cs). El API ya recibe `idCotizacion`;
// al regenerar los contratos este archivo se borra.
//
// Proforma (cotización) que esta factura convierte. El API la valida (aceptada, sin facturar, mismo cliente) y la marca
// como facturada en la MISMA transacción que crea la venta. null = factura normal.

public partial class FacturaDTO
{
    [JsonPropertyName("idCotizacion")]
    public long? IdCotizacion { get; set; }
}
