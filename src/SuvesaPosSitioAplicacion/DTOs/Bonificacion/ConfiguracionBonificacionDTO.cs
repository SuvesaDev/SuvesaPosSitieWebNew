using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Bonificacion;

/// <summary>
/// Tipo del catalogo maestro de bonificaciones ("compra N, regalo M").
///
/// TEMPORAL — escrito a mano mientras los contratos NSwag no se regeneran contra
/// el API nuevo (endpoints /ConfiguracionBonificacion/* de §3.1). Al correr
/// <c>./tools/actualizar-contratos.sh</c> esto lo sustituye el DTO generado.
/// </summary>
public sealed class ConfiguracionBonificacionDTO
{
    [JsonPropertyName("idConfiguracionBonificacion")] public int IdConfiguracionBonificacion { get; set; }
    [JsonPropertyName("descripcion")] public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("cantidadVenta")] public int CantidadVenta { get; set; }
    [JsonPropertyName("cantidadBonificable")] public int CantidadBonificable { get; set; }
    [JsonPropertyName("activo")] public bool Activo { get; set; } = true;

    /// <summary>
    /// Unidades totales a facturar para completar el grupo: las que se pagan
    /// (<see cref="CantidadVenta"/>) más las que se regalan (<see cref="CantidadBonificable"/>).
    /// "Compra 10 lleva 1" factura 11 unidades (10 pagadas + 1 gratis), no 10.
    /// </summary>
    [JsonIgnore]
    public int CantidadTotalGrupo => CantidadVenta + CantidadBonificable;
}
