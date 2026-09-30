using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// El catálogo es configurable: se puede corregir código, tipo, naturaleza y padre
/// mientras la cuenta no tenga pólizas. Estos campos viajan en el mismo DTO generado;
/// el cliente NSwag los serializa porque son propiedades públicas del parcial.
/// </summary>
public partial class EditarCuentaContableDTO
{
    [JsonPropertyName("codigo")]
    public string? Codigo { get; set; }

    [JsonPropertyName("naturaleza")]
    public string? Naturaleza { get; set; }

    [JsonPropertyName("tipo")]
    public string? Tipo { get; set; }

    [JsonPropertyName("permiteMovimiento")]
    public bool? PermiteMovimiento { get; set; }

    [JsonPropertyName("idCuentaPadre")]
    public long? IdCuentaPadre { get; set; }

    [JsonPropertyName("actualizarPadre")]
    public bool ActualizarPadre { get; set; }
}
