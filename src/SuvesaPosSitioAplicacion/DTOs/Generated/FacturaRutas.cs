using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>Metadatos comerciales internos; no forman parte del comprobante fiscal.</summary>
public partial class FacturaDTO
{
    [JsonPropertyName("idRutaComercial")] public int? IdRutaComercial { get; set; }
    [JsonPropertyName("idUsuarioAgente")] public string? IdUsuarioAgente { get; set; }
    /// <summary>Usuario cuya clave interna habilitó esta venta.</summary>
    [JsonPropertyName("idUsuarioValidado")] public string? IdUsuarioValidado { get; set; }
    /// <summary>false fuerza que la venta no entre a despacho (tiquete de mostrador);
    /// null deja que el API lo tome de la ficha del cliente.</summary>
    [JsonPropertyName("requiereDespacho")] public bool? RequiereDespacho { get; set; }
}
