using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>Contrato temporal hasta regenerar NSwag para elegir la ruta de devolución.</summary>
public partial class FacturaDTO
{
    [JsonPropertyName("naturalezaFiscalDoc")]
    public string? NaturalezaFiscalDoc { get; set; }
}
