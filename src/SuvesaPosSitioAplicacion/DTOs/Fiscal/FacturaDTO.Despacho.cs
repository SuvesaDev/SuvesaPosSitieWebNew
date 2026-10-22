using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado. Al regenerar NSwag, el generado
// ya trae `requiereDespacho` y este archivo se borra.

public partial class FacturaDTO
{
    [JsonPropertyName("requiereDespacho")]
    public bool? RequiereDespacho { get; set; }
}
