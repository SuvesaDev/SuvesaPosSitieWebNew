using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
public interface ITiposExoneracionFiscales
{
    Task<ResponseGeneric<ICollection<TipoExoneracionFiscalDTO>>> Obtener();
    Task<ResponseGeneric<TipoExoneracionFiscalDTO>> Crear(TipoExoneracionFiscalDTO tipo);
    Task<ResponseGeneric<TipoExoneracionFiscalDTO>> Actualizar(TipoExoneracionFiscalDTO tipo);
}
