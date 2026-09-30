using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

public partial class ProveedorDTO
{
    [JsonPropertyName("porcentajeRetencion")]
    public decimal? PorcentajeRetencion { get; set; }
}
