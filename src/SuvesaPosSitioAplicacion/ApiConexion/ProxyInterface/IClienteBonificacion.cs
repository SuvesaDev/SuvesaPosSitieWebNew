using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Bonificacion asignada a un cliente. Cada registro ya trae el tipo de
/// bonificacion Y el articulo asociado juntos (<see cref="ClienteBonificacionConfiguracionDTO.IdArticulo"/>);
/// no hay una lista separada de "productos" en el API real, a diferencia de lo
/// que muestra el sistema actual (que ademas llama a dos endpoints —
/// ClienteBonificacion/CreateArticulo y /GetArticulos— que no existen en el
/// API: cualquier alta de "producto de bonificacion" en el sistema actual
/// falla con 404 hoy mismo).
/// </summary>
public interface IClienteBonificacion
{
    Task<ResponseGeneric<ICollection<ClienteBonificacionConfiguracionDTO>>> ObtenerPorCliente(string? cedula, long identificacion);

    Task<ResponseGeneric<ClienteBonificacionConfiguracionDTO>> Crear(ClienteBonificacionConfiguracionDTO configuracion);

    Task<ResponseGeneric<ClienteBonificacionConfiguracionDTO>> Editar(ClienteBonificacionConfiguracionDTO configuracion);

    Task<ResponseGeneric<bool>> Eliminar(int idConfiguracion);
}
