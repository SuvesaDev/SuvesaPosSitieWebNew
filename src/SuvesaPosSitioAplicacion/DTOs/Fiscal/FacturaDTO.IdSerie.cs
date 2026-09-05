using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (PLAN_TIQUETE_RUTAS_FACTURACION_WEB.md W1/W3;
// el regen completo de contratos rompe ~50 proxies porque el API divergió). Al regenerar,
// el generado ya trae `idSerie` y este archivo se borra.
//
// A1.3 del API: la pantalla manda la Serie exacta elegida por el usuario. Si es null el
// API resuelve por tipo+terminal+condición (comportamiento legado); si viene > 0 valida
// que corresponda al emisor/sucursal/tipo/condición y no hace fallback.

public partial class FacturaDTO
{
    [JsonPropertyName("idSerie")]
    public int? IdSerie { get; set; }
}
