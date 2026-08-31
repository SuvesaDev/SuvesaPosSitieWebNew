using SuvesaPosSitioAplicacion.Class;

namespace SuvesaPosSitioAplicacion.Security;

/// <summary>
/// Datos de la sesion del usuario durante la vida del circuito de Blazor.
///
/// DESVIACION DELIBERADA respecto a FCRCASitioAplicacion: alli el token se lee de
/// <c>ISession</c> con <c>IHttpContextAccessor</c>. En Blazor Server eso no sirve,
/// porque el HttpContext solo existe durante el render inicial y desaparece en cuanto
/// arranca el circuito. El sentido es el mismo (el token vive en el servidor y el
/// navegador nunca lo ve); la implementacion lee del ticket de autenticacion.
///
/// Rediseno de seguridad V2: los permisos casan por <b>codigo de funcion</b>
/// (<c>MODULO.SLUG</c>), no por rotulo.
/// </summary>
public interface IContextoSesion
{
    bool Autenticado { get; }
    string? Token { get; }
    string? Usuario { get; }

    /// <summary>Perfil SUPER_ADMIN: ve todo y no pasa por rol.</summary>
    bool EsSuperAdministrador { get; }

    /// <summary>Alias historico de <see cref="EsSuperAdministrador"/> para las Views que ya lo usan.</summary>
    bool EsAdministrador { get; }

    /// <summary>Codigo del perfil (SUPER_ADMIN / ADMIN / USUARIO / ...). Vacio si no hay.</summary>
    string? PerfilCodigo { get; }

    bool EsCostaPets { get; }
    bool EsAgenteCostaPets { get; }

    int IdSucursal { get; }
    string? NombreSucursal { get; }
    bool TieneSucursal { get; }

    /// <summary>Codigos de modulo presentes en los permisos del rol.</summary>
    IReadOnlyCollection<string> Menus { get; }
    IReadOnlyCollection<PermisoFuncion> Permisos { get; }

    /// <summary>Si la funcion puede abrirse. Equivale a la accion VER.</summary>
    bool PuedeVer(string funcionCodigo);

    /// <summary>
    /// Si el rol tiene siquiera mencionada esta funcion. Una funcion no mencionada
    /// no es lo mismo que una denegada mientras el catalogo no este completo
    /// (bandera <c>SeePos:VerPantallasNoGobernadas</c>).
    /// </summary>
    bool EstaGobernada(string funcionCodigo);

    /// <summary>Si el usuario puede ejecutar una accion concreta sobre una funcion.</summary>
    bool Puede(string funcionCodigo, AccionPantalla accion);

    /// <summary>Carga los datos desde el usuario autenticado. Idempotente.</summary>
    Task CargarAsync();
}
