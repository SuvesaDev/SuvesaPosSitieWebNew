using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// TEMPORAL — añadido a mano al DTO generado (el regen completo de contratos
// rompe ~50 proxies porque el API divergió). Al regenerar, el generado ya trae
// `codigoActividad`/`actividades` y este archivo se borra.
//
// Código de actividad económica del cliente (Hacienda). String libre; se
// captura en la pantalla de clientes, apartado "Facturación electrónica".
// Se mantiene por compatibilidad con la factura electrónica, que sigue
// usando este campo único como actividad del receptor — el detalle completo
// (un cliente puede tener varias actividades) vive en `Actividades`.

public partial class ClienteDTO
{
    [JsonPropertyName("codigoActividad")]
    public string? CodigoActividad { get; set; }

    [JsonPropertyName("actividades")]
    public System.Collections.Generic.List<ActividadEconomicaClienteDTO> Actividades { get; set; } = new();
}

public partial class FiltranClienteDTO
{
    [JsonPropertyName("codigoActividad")]
    public string? CodigoActividad { get; set; }

    /// <summary>Sucursal + nombre comercial de cada fila de facturación del cliente,
    /// unidos, para poder filtrar la lista por esos textos. Se borra al regenerar NSwag.</summary>
    [JsonPropertyName("sucursalesTexto")]
    public string? SucursalesTexto { get; set; }
}

/// <summary>Una actividad económica del cliente (Hacienda). Mismo contrato que
/// ActividadesClienteDTO en el API.</summary>
public sealed class ActividadEconomicaClienteDTO
{
    [JsonPropertyName("codigo")]
    public string? Codigo { get; set; }

    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }

    [JsonPropertyName("activo")]
    public bool? Activo { get; set; } = true;

    [JsonPropertyName("principal")]
    public bool? Principal { get; set; }

    [JsonPropertyName("idCliente")]
    public long? IdCliente { get; set; }
}
