using System.Text.Json.Serialization;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (el regen completo de contratos
// rompe ~50 proxies). Al regenerar, el generado ya trae estos campos y este
// archivo se borra.
//
// Clasificación de uso del tipo de documento (reemplaza a los bool
// Compra/Consignacion, que quedan sin poblar).

public partial class TipoFactura
{
    [JsonPropertyName("uso")]
    public UsoTipoDocumento Uso { get; set; } = UsoTipoDocumento.Facturacion;

    [JsonPropertyName("contado")]
    public bool Contado { get; set; }

    [JsonPropertyName("activo")]
    public bool Activo { get; set; } = true;

    // PK del tipo (TiposFactura.Id) y su código FE — necesarios para casar un
    // perfil de emisión con su tipo (SANEAMIENTO Fase 8.3).
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("codigoFE")]
    public string? CodigoFe { get; set; }
}
