using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// §5.9 — la asignación de bonificación del cliente vuelve a llevar artículo
/// (`IdArticulo`, ya presente en el DTO generado) y gana una nota corta.
///
/// TEMPORAL — añadido a mano al DTO generado (el regen completo de contratos
/// rompe ~50 proxies porque el API divergió). Al regenerar, el generado ya trae
/// `descripcion` y este archivo se borra. `DescripcionArticulo` (sólo lectura,
/// el nombre del artículo) ya viene en el generado.
/// </summary>
public partial class ClienteBonificacionConfiguracionDTO
{
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
}
