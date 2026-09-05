using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Consulta y mantenimiento base de inventario.
/// </summary>
public interface IInventarioConsulta
{
    /// <summary>Busca por descripcion o por codigo, segun lo que se escriba. Si se
    /// indica <paramref name="idBodega"/>, la Existencia devuelta es la de esa
    /// bodega y no el acumulado global del artículo.</summary>
    Task<ResponseGeneric<ICollection<InventarioDTO>>> Buscar(string texto, bool incluirInhabilitados = false, int? idBodega = null);

    /// <summary>
    /// Trae el listado de articulos sin termino de busqueda (la pantalla lo carga al
    /// abrir y filtra en cliente). El API decide el tope de filas.
    /// </summary>
    Task<ResponseGeneric<ICollection<InventarioDTO>>> Listar(bool incluirInhabilitados = false);

    /// <summary>Busca articulos marcados como MAG por codigo o descripcion.</summary>
    Task<ResponseGeneric<ICollection<InventarioDTO>>> BuscarMag(string texto);

    /// <summary>Lotes de un articulo, con su vencimiento y existencia.</summary>
    Task<ResponseGeneric<ICollection<StockLoteDTO>>> Lotes(long idArticulo);

    Task<ResponseGeneric<StockLoteDTO>> CrearLote(StockLoteDTO lote);

    Task<ResponseGeneric<bool>> EliminarLote(long idLote);

    Task<ResponseGeneric<InventarioDTO>> Uno(long codigo);

    Task<ResponseGeneric<InventarioDTO>> Crear(InventarioDTO articulo);

    Task<ResponseGeneric<InventarioDTO>> Editar(InventarioDTO articulo);

    Task<ResponseGeneric<InventarioDTO>> CambiarEstado(EliminarInventarioDTO articulo, bool activar);

    Task<ResponseGeneric<CodigoBarrasInventarioDTO>> EliminarCodigoBarras(CodigoBarrasInventarioDTO codigo);

    /// <summary>
    /// Ajuste directo de existencia (solo CostaPets, ver sistema actual). Es una
    /// escritura inmediata, separada del "Guardar" del artículo — igual que hace
    /// el sistema actual.
    /// </summary>
    Task<ResponseGeneric<bool>> ActualizarExistencia(int codArticulo, float cantidad, int codBodega = 0);

    /// <summary>
    /// Actualiza el costo guardado de un artículo (por su código de negocio,
    /// no el código interno). Se usa cuando el costo calculado a partir de la
    /// fórmula del artículo quedó por encima del costo guardado.
    /// </summary>
    Task<ResponseGeneric<bool>> ActualizarCosto(string codArticulo, double costoNuevo);
}
