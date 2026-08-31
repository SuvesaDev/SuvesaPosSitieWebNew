using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface ITiposIdentificacionFiscales
{
    Task<ResponseGeneric<ICollection<TipoIdentificacionFiscalDTO>>> Obtener();
    Task<ResponseGeneric<TipoIdentificacionFiscalDTO>> Crear(TipoIdentificacionFiscalDTO tipo);
    Task<ResponseGeneric<TipoIdentificacionFiscalDTO>> Actualizar(TipoIdentificacionFiscalDTO tipo);
    Task<ResponseGeneric<TipoIdentificacionFiscalDTO>> Deshabilitar(TipoIdentificacionFiscalDTO tipo);
}
