using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// Respuesta de GET inventario/UltimoCodigoArticulo: código del último artículo
/// creado, como referencia para el consecutivo manual. Se borra al regenerar el
/// contrato NSwag.
/// </summary>
public sealed class UltimoCodigoArticuloDTO
{
    [JsonPropertyName("codigo")]
    public string? Codigo { get; set; }
}
