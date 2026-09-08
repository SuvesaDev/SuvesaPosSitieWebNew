using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IDenominacionesMonedaFiscales
{
    Task<ResponseGeneric<ICollection<DenominacionMonedaFiscalDTO>>> Obtener();
    Task<ResponseGeneric<DenominacionMonedaFiscalDTO>> Crear(DenominacionMonedaFiscalDTO denominacion);
    Task<ResponseGeneric<DenominacionMonedaFiscalDTO>> Actualizar(DenominacionMonedaFiscalDTO denominacion);
    Task<ResponseGeneric<DenominacionMonedaFiscalDTO>> Deshabilitar(long id);
}
