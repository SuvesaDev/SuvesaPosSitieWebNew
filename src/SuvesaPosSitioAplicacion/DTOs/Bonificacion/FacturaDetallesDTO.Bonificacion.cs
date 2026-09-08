using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// §3.3 — campos de agrupación de bonificación en la línea de factura.
///
/// TEMPORAL — añadidos a mano al DTO generado mientras los contratos NSwag no se
/// regeneran contra el API nuevo (el contrato del API divergió demasiado del que
/// consume el sitio; un regen completo rompe ~50 proxies). Al regenerar, el
/// generado ya trae estos campos y este archivo se borra.
/// </summary>
public partial class FacturaDetallesDTO
{
    /// <summary>Id del grupo de bonificación, único dentro de la factura. Null si la línea no es de bonificación.</summary>
    [JsonPropertyName("idGrupoBonificacion")]
    public int? IdGrupoBonificacion { get; set; }

    /// <summary>Tipo del catálogo de bonificación aplicado al grupo.</summary>
    [JsonPropertyName("idConfiguracionBonificacion")]
    public int? IdConfiguracionBonificacion { get; set; }

    /// <summary>true = línea "principal" del grupo: la que dispara la bonificación y la única que el cajero puede eliminar (borra en cascada el resto).</summary>
    [JsonPropertyName("esLineaPrincipalBonificacion")]
    public bool? EsLineaPrincipalBonificacion { get; set; }
}
