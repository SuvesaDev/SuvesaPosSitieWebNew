using SuvesaPosSitioAplicacion.DTOs.Parametros;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

public interface IMantenimientosInventario
{
    Task<ResponseGeneric<ICollection<BodegaMantenimientoDTO>>> Bodegas();
    Task<ResponseGeneric<BodegaMantenimientoDTO>> CrearBodega(BodegaMantenimientoDTO bodega);
    Task<ResponseGeneric<BodegaMantenimientoDTO>> EditarBodega(BodegaMantenimientoDTO bodega);
    Task<ResponseGeneric<bool>> DesactivarBodega(int idBodega);
    Task<ResponseGeneric<ICollection<AreaMantenimientoDTO>>> Areas();
    Task<ResponseGeneric<AreaMantenimientoDTO>> CrearArea(AreaMantenimientoDTO area);
    Task<ResponseGeneric<AreaMantenimientoDTO>> EditarArea(AreaMantenimientoDTO area);
    Task<ResponseGeneric<bool>> EliminarArea(decimal idArea);
}
