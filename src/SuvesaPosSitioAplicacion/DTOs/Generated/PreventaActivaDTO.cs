using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// Fila del listado de preventas pendientes de cobro/facturación (pantalla Cobrar).
/// Se borra al regenerar el contrato NSwag.
/// </summary>
public sealed class PreventaActivaDTO
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("ficha")] public int Ficha { get; set; }
    [JsonPropertyName("codCliente")] public long CodCliente { get; set; }
    [JsonPropertyName("numFactura")] public string? NumFactura { get; set; }
    [JsonPropertyName("cliente")] public string? Cliente { get; set; }
    [JsonPropertyName("fecha")] public string? Fecha { get; set; }
    [JsonPropertyName("total")] public double Total { get; set; }
    [JsonPropertyName("tipo")] public int Tipo { get; set; }
}
