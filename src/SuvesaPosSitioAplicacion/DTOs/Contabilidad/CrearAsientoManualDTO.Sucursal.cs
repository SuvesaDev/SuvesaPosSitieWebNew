using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

public partial class CrearAsientoManualDTO
{
    [JsonPropertyName("idEmisor")]
    public int IdEmisor { get; set; }

    [JsonPropertyName("idSucursal")]
    public int IdSucursal { get; set; }

    [JsonPropertyName("idEmpresa")]
    public long? IdEmpresa { get; set; }

    /// <summary>Descripción de la póliza; si falta el API guarda "Póliza manual".</summary>
    [JsonPropertyName("concepto")]
    public string? Concepto { get; set; }
}

public partial class LineaAsientoContableDTO
{
    [JsonPropertyName("idSucursal")]
    public int? IdSucursal { get; set; }
}
