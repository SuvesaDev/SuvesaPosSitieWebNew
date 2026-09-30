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
}

public partial class LineaAsientoContableDTO
{
    [JsonPropertyName("idSucursal")]
    public int? IdSucursal { get; set; }
}
