using SuvesaPosSitioAplicacion.DTOs.Fiscal; using SuvesaPosSitioAplicacion.Helpers;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
public interface ITiposCobroFiscales { Task<ResponseGeneric<ICollection<TipoCobroFiscalDTO>>> Obtener(); Task<ResponseGeneric<TipoCobroFiscalDTO>> Crear(TipoCobroFiscalDTO tipo); Task<ResponseGeneric<TipoCobroFiscalDTO>> Actualizar(TipoCobroFiscalDTO tipo); Task<ResponseGeneric<TipoCobroFiscalDTO>> Deshabilitar(int id); }
