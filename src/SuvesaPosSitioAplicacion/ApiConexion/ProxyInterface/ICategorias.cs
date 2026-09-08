using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Categorias de inventario. El API solo ofrece crear y listar; no hay edicion.
/// (DesactivarCategoriaxInventario no desactiva la categoria: desactiva una
/// relacion categoria-articulo, un concepto distinto que no pertenece aqui.)
/// </summary>
public interface ICategorias
{
    Task<ResponseGeneric<ICollection<CategoriasDTO>>> Obtener();

    Task<ResponseGeneric<CategoriasDTO>> Crear(CategoriasDTO categoria);
}
