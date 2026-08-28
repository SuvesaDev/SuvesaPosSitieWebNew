using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Administracion de usuarios del sistema.
///
/// Se pasa siempre la clave en el cuerpo del DTO, nunca en la URL. El sistema
/// actual valida la clave con GET /usuario/ValidarExisteContrasena?contrasena=...,
/// que la manda en texto plano dentro de la URL (queda en logs de acceso del
/// servidor). Esa llamada no se replica aqui.
/// </summary>
public interface IUsuarios
{
    Task<ResponseGeneric<ICollection<BuscarUsuarioDTO>>> Buscar(string? texto);

    /// <summary>
    /// El resultado de <see cref="Buscar"/> NO trae un identificador numerico, solo
    /// el usuario de acceso (texto). El sistema actual pide el detalle con ESE texto
    /// en el parametro "id" del endpoint (que el OpenAPI declara como numerico), y
    /// solo tras esa respuesta obtiene el Id numerico real, que usa despues para
    /// editar. Se replica ese mismo camino aqui.
    ///
    /// SIN VERIFICAR contra devapi.pos2650.com: la base de desarrollo no tiene
    /// usuarios sembrados aparte del propio login de pruebas, asi que no hay forma
    /// de confirmar el comportamiento con datos reales. Si al usarlo en desarrollo
    /// esto falla, es la primera sospecha a revisar.
    /// </summary>
    Task<ResponseGeneric<UsuariosDTO>> ObtenerUno(string idUsuario);

    Task<ResponseGeneric<UsuariosDTO>> Crear(UsuariosDTO usuario);

    Task<ResponseGeneric<UsuarioDTO>> Editar(long id, UsuarioDTO usuario);

    Task<ResponseGeneric<ICollection<Perfil>>> ObtenerPerfiles();

    Task<ResponseGeneric<ICollection<Role>>> ObtenerRoles();
}
