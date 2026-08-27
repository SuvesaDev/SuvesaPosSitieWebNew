using SuvesaPosSitioAplicacion.Class;

namespace SuvesaPosSitioAplicacion.Security;

/// <summary>
/// Datos de la sesion del usuario durante la vida del circuito de Blazor.
///
/// DESVIACION DELIBERADA respecto a FCRCASitioAplicacion: alli el token se lee de
/// <c>ISession</c> con <c>IHttpContextAccessor</c>. En Blazor Server eso no sirve,
/// porque el HttpContext solo existe durante el render inicial y desaparece en cuanto
/// arranca el circuito. El sentido es el mismo (el token vive en el servidor y el
/// navegador nunca lo ve); la implementacion lee del ticket de autenticacion, que
/// tambien esta en servidor.
/// </summary>
public interface IContextoSesion
{
    bool Autenticado { get; }
    string? Token { get; }
    string? Usuario { get; }
    bool EsAdministrador { get; }

    int IdSucursal { get; }
    string? NombreSucursal { get; }
    bool TieneSucursal { get; }

    IReadOnlyCollection<string> Menus { get; }
    IReadOnlyCollection<PermisoPantalla> Permisos { get; }

    /// <summary>Si la pantalla puede abrirse. Equivale a la accion Ver.</summary>
    bool PuedeVer(string pantalla);

    /// <summary>Si el usuario puede ejecutar una accion concreta sobre una pantalla.</summary>
    bool Puede(string pantalla, AccionPantalla accion);

    /// <summary>Carga los datos desde el usuario autenticado. Idempotente.</summary>
    Task CargarAsync();
}
