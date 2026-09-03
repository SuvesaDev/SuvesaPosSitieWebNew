using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IMantenimientosInventario
{
    Task<ResponseGeneric<ICollection<Bodega>>> Bodegas();
}
