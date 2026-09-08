using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Orden de compra manual: alta, edición, anulación y búsqueda por proveedor.</summary>
public interface IOrdenesCompra
{
    Task<ResponseGeneric<ICollection<OrdenCompraDTO>>> BuscarPorProveedor(long idProveedor);

    Task<ResponseGeneric<OrdenCompraDTO>> Obtener(long idOrdenCompra);

    Task<ResponseGeneric<OrdenCompraDTO>> Crear(OrdenCompraDTO orden);

    Task<ResponseGeneric<OrdenCompraDTO>> Editar(OrdenCompraDTO orden);

    Task<ResponseGeneric<OrdenCompraDTO>> Anular(long idOrdenCompra);
}
