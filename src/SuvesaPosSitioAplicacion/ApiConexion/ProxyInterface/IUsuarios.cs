using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Seguridad;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Administracion de usuarios del sistema (rediseno de seguridad V2).
///
/// El alta pasa por <c>/seguridad/usuarios</c> con las compuertas del servidor
/// (perfil del llamante, exigencia de rol, compuerta SUPER_ADMIN). El perfil y el
/// rol de un usuario ya creado se cambian con endpoints propios
/// (<see cref="CambiarPerfil"/> / <see cref="CambiarRol"/>).
///
/// La clave viaja siempre en el cuerpo, nunca en la URL.
/// </summary>
public interface IUsuarios
{
    Task<ResponseGeneric<ICollection<BuscarUsuarioDTO>>> Buscar(string? texto);

    /// <summary>
    /// El resultado de <see cref="Buscar"/> NO trae id numerico, solo el usuario de
    /// acceso (texto). El API pide el detalle con ESE texto en el parametro "id"
    /// (que el OpenAPI declara numerico) y de ahi sale el Id real para editar.
    /// </summary>
    Task<ResponseGeneric<UsuarioDetalleDTO>> ObtenerUno(string idUsuario);

    /// <summary>Alta contra <c>/seguridad/usuarios</c>. <c>IdPerfil</c> obligatorio.</summary>
    Task<ResponseGeneric<UsuarioAltaDTO>> Crear(UsuarioAltaDTO usuario);

    Task<ResponseGeneric<UsuarioDTO>> Editar(long id, UsuarioDTO usuario);

    /// <summary>Cambia el perfil de un usuario. La compuerta SUPER_ADMIN la aplica el API.</summary>
    Task<ResponseGeneric<bool>> CambiarPerfil(long id, int idPerfil);

    Task<ResponseGeneric<bool>> CambiarRol(long id, int? idRol);

    /// <summary>Autoservicio: el usuario autenticado cambia SU clave interna.</summary>
    Task<ResponseGeneric<bool>> CambiarClaveInterna(string actual, string nueva);

    /// <summary>Autoservicio: el usuario autenticado cambia SU contraseña de ingreso.</summary>
    Task<ResponseGeneric<bool>> CambiarContrasenaIngreso(string actual, string nueva);
}
