using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Consulta y mantenimiento base de inventario.
/// </summary>
public interface IInventarioConsulta
{
    /// <summary>Busca por descripcion o por codigo, segun lo que se escriba.</summary>
    Task<ResponseGeneric<ICollection<InventarioDTO>>> Buscar(string texto, bool incluirInhabilitados = false);

    /// <summary>Busca articulos marcados como MAG por codigo o descripcion.</summary>
    Task<ResponseGeneric<ICollection<InventarioDTO>>> BuscarMag(string texto);

    /// <summary>Lotes de un articulo, con su vencimiento y existencia.</summary>
    Task<ResponseGeneric<ICollection<StockLoteDTO>>> Lotes(long idArticulo);

    Task<ResponseGeneric<InventarioDTO>> Uno(long codigo);

    Task<ResponseGeneric<InventarioDTO>> Crear(InventarioDTO articulo);

    Task<ResponseGeneric<InventarioDTO>> Editar(InventarioDTO articulo);

    Task<ResponseGeneric<InventarioDTO>> CambiarEstado(EliminarInventarioDTO articulo, bool activar);

    Task<ResponseGeneric<CodigoBarrasInventarioDTO>> EliminarCodigoBarras(CodigoBarrasInventarioDTO codigo);
}
