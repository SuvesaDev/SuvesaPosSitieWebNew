using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Devoluciones de compra (espejo de Devoluciones de venta, pero sin compuerta de clave interna: el sistema actual no la pide aqui).</summary>
public interface IDevolucionesCompra
{
    Task<ResponseGeneric<ICollection<DevolucionCompraDTO>>> Buscar(FiltroFacturaDevCompras filtro);

    Task<ResponseGeneric<DevolucionCompraDTO>> ObtenerUna(long id);

    Task<ResponseGeneric<DevolucionCompraDTO>> Crear(DevolucionCompraDTO devolucion);
}
