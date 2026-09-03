using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (el regen completo de contratos
// rompe ~50 proxies porque el API divergió). Al regenerar, el generado ya trae
// `codigoActividad` y este archivo se borra.
//
// Código de actividad económica del cliente (Hacienda). String libre; se
// captura en la pantalla de clientes, apartado "Facturación electrónica".

public partial class ClienteDTO
{
    [JsonPropertyName("codigoActividad")]
    public string? CodigoActividad { get; set; }
}

public partial class FiltranClienteDTO
{
    [JsonPropertyName("codigoActividad")]
    public string? CodigoActividad { get; set; }
}
