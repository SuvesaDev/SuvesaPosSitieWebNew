using SuvesaPosSitioAplicacion.Class;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.E2E;

/// <summary>
/// Contexto de sesion con un token fijo, para poder ejercitar los proxies fuera de
/// una peticion web. En la aplicacion real lo aporta el circuito o el HttpContext.
/// </summary>
public sealed class SesionFija : IContextoSesion
{
    public SesionFija(string? token = null) => Token = token;

    public string? Token { get; set; }

    public bool Autenticado => !string.IsNullOrWhiteSpace(Token);
    public string? Usuario => "pruebas";
    public bool EsAdministrador => true;
    public bool EsCostaPets => false;
    public bool EsAgenteCostaPets => false;
    public int IdSucursal => 0;
    public string? NombreSucursal => null;
    public bool TieneSucursal => false;
    public IReadOnlyCollection<string> Menus => Array.Empty<string>();
    public IReadOnlyCollection<PermisoPantalla> Permisos => Array.Empty<PermisoPantalla>();

    public bool EstaGobernada(string pantalla) => true;

    public bool PuedeVer(string pantalla) => true;
    public bool Puede(string pantalla, AccionPantalla accion) => true;
    public Task CargarAsync() => Task.CompletedTask;
}
