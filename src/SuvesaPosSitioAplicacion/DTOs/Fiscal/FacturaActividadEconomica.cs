using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (el regen completo de contratos
// rompe ~50 proxies). Al regenerar, el generado ya trae estos campos y este
// archivo se borra.
//
// Actividades económicas elegidas en Facturación que viajan al comprobante
// electrónico 4.4 (CodigoActividadEmisor / CodigoActividadReceptor).

public partial class FacturaDTO
{
    [JsonPropertyName("codigoActividadEmisor")]
    public string? CodigoActividadEmisor { get; set; }

    [JsonPropertyName("codigoActividadReceptor")]
    public string? CodigoActividadReceptor { get; set; }
}
