using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
public interface IFormasPagoFiscales { Task<ResponseGeneric<ICollection<FormasPagoDTO>>> Obtener(); Task<ResponseGeneric<FormasPagoDTO>> Crear(FormasPagoDTO forma); Task<ResponseGeneric<FormasPagoDTO>> Actualizar(FormasPagoDTO forma); }
