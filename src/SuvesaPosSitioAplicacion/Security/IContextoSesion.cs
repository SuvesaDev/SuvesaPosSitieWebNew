namespace SuvesaPosSitioAplicacion.Security;

/// <summary>
/// Datos de la sesion del usuario durante la vida del circuito de Blazor.
///
/// DESVIACION DELIBERADA respecto a FCRCASitioAplicacion: alli el token se guarda
/// en <c>ISession</c> y se lee con <c>IHttpContextAccessor</c>. En Blazor Server eso
/// no sirve, porque el HttpContext solo existe durante el render inicial y desaparece
/// en cuanto arranca el circuito. El sentido es el mismo (el token vive en el servidor
/// y el navegador nunca lo ve), pero la implementacion es un servicio con scope de
/// circuito, sembrado en el inicio de sesion.
/// </summary>
public interface IContextoSesion
{
    bool Autenticado { get; }
    string? Token { get; }
    string? Usuario { get; }
    int IdSucursal { get; }
    string? NombreSucursal { get; }
    bool EsAdministrador { get; }

    IReadOnlyCollection<string> Modulos { get; }
    IReadOnlyCollection<string> Pantallas { get; }

    bool PuedeVer(string pantalla);
    bool Puede(string pantalla, string accion);

    void Iniciar(DatosSesion datos);
    void Cerrar();
}

/// <summary>Lo que el inicio de sesion deja establecido para el resto del circuito.</summary>
public sealed record DatosSesion(
    string Token,
    string Usuario,
    bool EsAdministrador,
    int IdSucursal,
    string NombreSucursal,
    IReadOnlyCollection<string> Modulos,
    IReadOnlyCollection<string> Pantallas,
    IReadOnlyDictionary<string, IReadOnlyCollection<string>> AccionesPorPantalla);
