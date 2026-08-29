using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Matriz de permisos por rol: que pantallas puede ver/crear/modificar/borrar cada
/// rol. El sistema actual exige volver a escribir la clave del usuario en sesion
/// antes de dejar tocar esta pantalla (no la clave del rol, la propia). Se replica
/// esa compuerta con <see cref="ValidarPasswordActual"/>.
/// </summary>
public interface IRoles
{
    Task<ResponseGeneric<ICollection<Role>>> Buscar();

    Task<ResponseGeneric<ICollection<Modulo>>> Modulos();

    Task<ResponseGeneric<ICollection<Ventanas>>> Pantallas(int idModulo);

    Task<ResponseGeneric<ConfiguracionRollDTO>> ObtenerUno(int idRol);

    Task<ResponseGeneric<bool>> Crear(ConfiguracionRollDTO configuracion);

    Task<ResponseGeneric<bool>> Editar(ConfiguracionRollDTO configuracion);

    /// <summary>
    /// Valida la clave del usuario que tiene la sesion abierta (no hace falta decir
    /// cual usuario: el token ya lo identifica). Es la compuerta que el sistema
    /// actual pone antes de permitir tocar roles.
    /// </summary>
    Task<ResponseGeneric<Usuario>> ValidarPasswordActual(string contrasena);
}
