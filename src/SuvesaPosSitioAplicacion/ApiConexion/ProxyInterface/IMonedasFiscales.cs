using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IMonedasFiscales
{
    Task<ResponseGeneric<ICollection<MonedaFiscalDTO>>> Obtener();
    Task<ResponseGeneric<MonedaFiscalDTO>> Crear(MonedaFiscalDTO moneda);
    Task<ResponseGeneric<MonedaFiscalDTO>> Actualizar(MonedaFiscalDTO moneda);
    Task<ResponseGeneric<MonedaFiscalDTO>> Deshabilitar(int codigo, string? usuario);
}
