using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Consulta de inventario. Solo lectura: no toca existencias ni precios.
/// La pantalla de mantenimiento completa llega en la Ola 2.
/// </summary>
public interface IInventarioConsulta
{
    /// <summary>Busca por descripcion o por codigo, segun lo que se escriba.</summary>
    Task<ResponseGeneric<ICollection<InventarioDTO>>> Buscar(string texto, bool incluirInhabilitados = false);

    /// <summary>Busca articulos marcados como MAG por codigo o descripcion.</summary>
    Task<ResponseGeneric<ICollection<InventarioDTO>>> BuscarMag(string texto);

    /// <summary>Lotes de un articulo, con su vencimiento y existencia.</summary>
    Task<ResponseGeneric<ICollection<StockLoteDTO>>> Lotes(long idArticulo);
}
