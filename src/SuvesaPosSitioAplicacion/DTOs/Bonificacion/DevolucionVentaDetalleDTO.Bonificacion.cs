using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// §3.6 — bonificación en la línea de devolución de venta.
///
/// TEMPORAL — añadidos a mano al DTO generado (el regen completo de contratos
/// rompe ~50 proxies porque el API divergió). Al regenerar, el generado ya trae
/// estos campos y este archivo se borra.
/// </summary>
public partial class DevolucionVentaDetalleDTO
{
    [JsonPropertyName("esBonificacion")]
    public bool EsBonificacion { get; set; }

    [JsonPropertyName("idGrupoBonificacion")]
    public int? IdGrupoBonificacion { get; set; }

    [JsonPropertyName("idConfiguracionBonificacion")]
    public int? IdConfiguracionBonificacion { get; set; }

    [JsonPropertyName("esLineaPrincipalBonificacion")]
    public bool? EsLineaPrincipalBonificacion { get; set; }
}
