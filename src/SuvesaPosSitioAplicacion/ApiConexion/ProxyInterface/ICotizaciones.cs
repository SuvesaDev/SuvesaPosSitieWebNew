using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Proformas y cotizaciones. Documento con encabezado y detalle.</summary>
public interface ICotizaciones
{
    Task<ResponseGeneric<ICollection<CotizacionesDTO>>> Obtener();

    Task<ResponseGeneric<CotizacionesDTO>> ObtenerPorId(long id);

    Task<ResponseGeneric<CotizacionesDTO>> Crear(CotizacionesDTO cotizacion);

    Task<ResponseGeneric<CotizacionesDTO>> EditarEncabezado(CotizacionesDTO cotizacion);

    Task<ResponseGeneric<CotizacionesDTO>> Anular(CotizacionesDTO cotizacion);
}
