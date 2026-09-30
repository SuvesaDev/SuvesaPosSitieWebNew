using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

public partial class EstadoActivacionContabilidadDTO
{
    [JsonPropertyName("idSucursal")]
    public int? IdSucursal { get; set; }

    [JsonPropertyName("sucursalHabilitada")]
    public bool SucursalHabilitada { get; set; }

    [JsonPropertyName("relacionVigente")]
    public bool RelacionVigente { get; set; }
}
