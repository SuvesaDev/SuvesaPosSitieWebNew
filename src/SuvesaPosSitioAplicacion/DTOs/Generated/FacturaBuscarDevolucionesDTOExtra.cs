using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// Campos que el API agregó a FacturaBuscarDevolucionesDTO para que la pantalla de
/// devoluciones distinga documentos con el mismo número (Num_Factura se reinicia por
/// serie/tipo). Se borra al regenerar el contrato NSwag.
/// </summary>
public partial class FacturaBuscarDevolucionesDTO
{
    [JsonPropertyName("tipo")]
    public int Tipo { get; set; }

    [JsonPropertyName("total")]
    public double Total { get; set; }

    [JsonPropertyName("estadoMh")]
    public string? EstadoMh { get; set; }
}
