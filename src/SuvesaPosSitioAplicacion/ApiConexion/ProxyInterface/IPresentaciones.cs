using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Presentaciones de articulo. El API tiene un solo endpoint de escritura
/// (PostPresentaciones) que crea o actualiza segun exista el nombre: no hay
/// distincion de crear/editar como en Bancos o Familias.
/// </summary>
public interface IPresentaciones
{
    Task<ResponseGeneric<ICollection<Presentacione>>> Obtener();

    Task<ResponseGeneric<Presentacione>> Guardar(PresentacionDTO presentacion);
}
