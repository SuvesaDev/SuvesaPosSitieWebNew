using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (mismo patrón que FacturaDTO.IdCotizacion.cs). El API ya recibe y devuelve
// `idClienteSucursal`; al regenerar los contratos este archivo se borra.
//
// Sucursal del cliente a la que se cotiza; Facturación la respeta al facturar la proforma. null = el cliente no tiene varias.

public partial class CotizacionesDTO
{
    [JsonPropertyName("idClienteSucursal")]
    public long? IdClienteSucursal { get; set; }
}
