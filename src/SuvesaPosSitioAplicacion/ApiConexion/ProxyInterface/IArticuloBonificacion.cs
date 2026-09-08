using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Tipos de bonificacion asignados a un articulo (el articulo "disparador":
/// comprar N de este articulo da derecho a M de regalo). No lleva un articulo
/// de regalo asociado en este DTO — eso se maneja aparte, con
/// <see cref="IArticulosRelacionados"/> marcando <c>EsRelacionBonificacion</c>.
/// </summary>
public interface IArticuloBonificacion
{
    Task<ResponseGeneric<ICollection<ArticuloBonificacionConfiguracion>>> ObtenerPorArticulo(long idInventario);

    Task<ResponseGeneric<ArticuloBonificacionConfiguracion>> Crear(ArticuloBonificacionConfiguracion configuracion);

    Task<ResponseGeneric<ArticuloBonificacionConfiguracion>> Editar(ArticuloBonificacionConfiguracion configuracion);

    Task<ResponseGeneric<bool>> Eliminar(int idConfiguracion);
}
