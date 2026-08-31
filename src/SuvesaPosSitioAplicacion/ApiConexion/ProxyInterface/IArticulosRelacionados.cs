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
}
