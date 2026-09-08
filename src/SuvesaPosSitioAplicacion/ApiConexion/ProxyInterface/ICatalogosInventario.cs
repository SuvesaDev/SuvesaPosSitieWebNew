using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Catálogos usados al editar un artículo, separados de su mantenimiento.</summary>
public interface ICatalogosInventario
{
    Task<ResponseGeneric<ICollection<SubFamiliasFilterInventarioDTO>>> Familias();

    Task<ResponseGeneric<ICollection<ProveedoresFilterInventarioDTO>>> Proveedores();

    Task<ResponseGeneric<ICollection<Presentacione>>> Presentaciones();

    Task<ResponseGeneric<ICollection<CabysArticulos>>> BuscarCabys(string texto);
}
