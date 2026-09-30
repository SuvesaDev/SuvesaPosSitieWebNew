using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Auditoria;

public sealed class FilaAuditoriaWebDTO
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("fechaUtc")] public DateTime FechaUtc { get; set; }
    [JsonPropertyName("accion")] public string Accion { get; set; } = "";
    [JsonPropertyName("entidad")] public string Entidad { get; set; } = "";
    [JsonPropertyName("entidadId")] public string EntidadId { get; set; } = "";
    [JsonPropertyName("usuario")] public string? Usuario { get; set; }
    [JsonPropertyName("direccionIp")] public string? DireccionIp { get; set; }
    [JsonPropertyName("valoresAnteriores")] public string? ValoresAnteriores { get; set; }
    [JsonPropertyName("valoresNuevos")] public string? ValoresNuevos { get; set; }
    [JsonPropertyName("camposCambiados")] public string? CamposCambiados { get; set; }
}
