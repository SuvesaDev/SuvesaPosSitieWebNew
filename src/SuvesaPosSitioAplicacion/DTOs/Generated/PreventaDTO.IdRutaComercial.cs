using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (igual que FacturaDTO.Pagos.cs). Al
// regenerar los contratos el generado ya trae `idRutaComercial` y este archivo se borra.

public partial class PreventaDTO
{
    /// <summary>Ruta comercial resuelta al crear la preventa. Hay que reenviarla en
    /// FacturarPreventaContadoComandoDTO.IdRutaComercial al cobrar — sin ella, un
    /// cajero SAC (que no tiene ruta propia) recibe "Seleccione la ruta de venta."
    /// al facturar, aunque la ruta ya se haya elegido al crear la preventa.</summary>
    [JsonPropertyName("idRutaComercial")]
    public int? IdRutaComercial { get; set; }
}
