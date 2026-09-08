using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Seguridad;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>
/// Mantenimiento de roles + su matriz de permisos, y del catalogo
/// (modulos / funciones / acciones). Rediseno de seguridad V2.
///
/// Reemplaza a <c>IRoles</c>. La pantalla sigue pidiendo reconfirmar la clave del
/// propio usuario antes de mostrar nada (<see cref="ValidarPasswordActual"/>).
/// </summary>
public interface IRolesPermisos
{
    // catalogo
    Task<ResponseGeneric<ICollection<ModuloCatalogoDTO>>> Catalogo();
    Task<ResponseGeneric<ICollection<AccionCatalogoDTO>>> Acciones();
    Task<ResponseGeneric<AccionCatalogoDTO>> GuardarAccion(AccionCatalogoDTO dto);
    Task<ResponseGeneric<ModuloCatalogoDTO>> GuardarModulo(ModuloCatalogoDTO dto);
    Task<ResponseGeneric<bool>> DesactivarModulo(int idModulo);
    Task<ResponseGeneric<ICollection<FuncionCatalogoDTO>>> Funciones(int idModulo);
    Task<ResponseGeneric<FuncionCatalogoDTO>> GuardarFuncion(FuncionCatalogoDTO dto);
    Task<ResponseGeneric<bool>> DesactivarFuncion(int idFuncion);
    Task<ResponseGeneric<bool>> GuardarAccionesDeFuncion(int idFuncion, IEnumerable<string> codigos);

    // roles
    Task<ResponseGeneric<ICollection<RolResumenDTO>>> Roles();
    Task<ResponseGeneric<RolDetalleDTO>> Rol(int idRol);
    Task<ResponseGeneric<RolDetalleDTO>> CrearRol(RolDetalleDTO dto);
    Task<ResponseGeneric<RolDetalleDTO>> EditarRol(int idRol, RolDetalleDTO dto);
    Task<ResponseGeneric<bool>> GuardarPermisos(int idRol, IEnumerable<PermisoFilaDTO> filas);

    /// <summary>Valida la clave del usuario en sesion (compuerta previa al mantenimiento).</summary>
    Task<ResponseGeneric<Usuario>> ValidarPasswordActual(string contrasena);
}
