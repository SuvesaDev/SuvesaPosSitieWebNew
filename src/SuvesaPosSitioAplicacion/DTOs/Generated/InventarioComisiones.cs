using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// Campos agregados al contrato de inventario mientras se regenera NSwag contra
/// el API. Son configuración comercial y no intervienen en la factura fiscal.
/// </summary>
public partial class InventarioDTO
{
    [JsonPropertyName("porcentajeComisionAgentes")]
    public float PorcentajeComisionAgentes { get; set; }

    [JsonPropertyName("porcentajeComisionServicioCliente")]
    public float PorcentajeComisionServicioCliente { get; set; }
}
