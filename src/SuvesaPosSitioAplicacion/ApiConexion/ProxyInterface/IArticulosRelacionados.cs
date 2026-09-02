using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
public interface IArticulosRelacionados {
    Task<ResponseGeneric<ICollection<ArticulosRelacionadosDTO>>> Buscar(long principal);
    Task<ResponseGeneric<bool>> Guardar(long principal, long relacionado, float cantidad, bool activo);

    // Mismos endpoints, variante "bonificacion": el articulo relacionado es el
    // regalo, no un articulo relacionado comun (empaques, combos, etc.).
    Task<ResponseGeneric<ICollection<ArticulosRelacionadosDTO>>> BuscarBonificacion(long principal);
    Task<ResponseGeneric<bool>> GuardarBonificacion(long principal, long relacionado, float cantidad, bool activo);

    /// <summary>
    /// Costo de armar un articulo de formula a partir de sus insumos (suma de
    /// costos de los componentes). Se usa para detectar cuando el costo
    /// guardado del articulo quedo desactualizado por debajo del real.
    /// </summary>
    Task<ResponseGeneric<double>> CostoCalculado(string codArticulo);
}
