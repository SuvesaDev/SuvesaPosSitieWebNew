using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SuvesaPosSitioAplicacion.Class;

namespace SuvesaPosSitioAplicacion.Security;

/// <inheritdoc cref="IContextoSesion" />
public sealed class ContextoSesion : IContextoSesion
{
    private readonly AuthenticationStateProvider _estado;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<ContextoSesion> _log;

    private ClaimsPrincipal? _usuario;
    private List<PermisoPantalla> _permisos = new();
    private Dictionary<string, PermisoPantalla> _porPantalla = new(StringComparer.OrdinalIgnoreCase);

    public ContextoSesion(
        AuthenticationStateProvider estado,
        IHttpContextAccessor http,
        ILogger<ContextoSesion> log)
    {
        _estado = estado;
        _http = http;
        _log = log;
    }

    public bool Autenticado => _usuario?.Identity?.IsAuthenticated ?? false;

    public string? Token => Claim(ClaimsSeePos.Token);
    public string? Usuario => _usuario?.Identity?.Name;
    public bool EsAdministrador => Claim(ClaimsSeePos.Administrador) == bool.TrueString;

    public int IdSucursal => int.TryParse(Claim(ClaimsSeePos.IdSucursal), out var id) ? id : 0;
    public string? NombreSucursal => Claim(ClaimsSeePos.NombreSucursal);
    public bool TieneSucursal => IdSucursal > 0;

    public IReadOnlyCollection<string> Menus =>
        _permisos.Select(p => p.Menu)
                 .Where(m => !string.IsNullOrWhiteSpace(m))
                 .Distinct(StringComparer.OrdinalIgnoreCase)
                 .ToList();

    public IReadOnlyCollection<PermisoPantalla> Permisos => _permisos;

    public bool PuedeVer(string pantalla) => Puede(pantalla, AccionPantalla.Ver);

    public bool Puede(string pantalla, AccionPantalla accion)
    {
        if (!Autenticado)
        {
            return false;
        }

        // El administrador entra a todo. Es como se comporta el sistema actual.
        if (EsAdministrador)
        {
            return true;
        }

        return _porPantalla.TryGetValue(pantalla, out var permiso) && permiso.Permite(accion);
    }

    public async Task CargarAsync()
    {
        if (_usuario is not null)
        {
            return;
        }

        // En render estatico el usuario esta en el HttpContext y es la fuente fiable.
        // En un circuito interactivo el HttpContext ya no existe, y entonces manda
        // AuthenticationStateProvider. Las dos rutas dan el mismo principal.
        var deHttp = _http.HttpContext?.User;

        if (deHttp?.Identity?.IsAuthenticated == true)
        {
            _usuario = deHttp;
        }
        else
        {
            try
            {
                var estado = await _estado.GetAuthenticationStateAsync();
                _usuario = estado.User;
            }
            catch (InvalidOperationException)
            {
                // Fuera del ambito de DI de un componente Razor no hay estado que
                // leer. Pasa durante el propio inicio de sesion, cuando todavia no
                // hay usuario. Se trata como "sin sesion", no como fallo.
                _usuario = new ClaimsPrincipal(new ClaimsIdentity());
                return;
            }
        }

        if (_usuario.Identity?.IsAuthenticated == true
            && string.IsNullOrWhiteSpace(_usuario.FindFirst(ClaimsSeePos.Token)?.Value))
        {
            // Sesion valida pero sin token del API: las llamadas van a fallar con 401
            // y el motivo no se veria por ningun lado. Mejor dejarlo dicho.
            _log.LogWarning(
                "La sesion de {Usuario} no lleva token del API. Las llamadas respondera 401.",
                _usuario.Identity.Name);
        }

        _permisos = _usuario.FindAll(ClaimsSeePos.Permiso)
                            .Select(c => PermisoPantalla.DesdeClaim(c.Value))
                            .Where(p => p is not null)
                            .Select(p => p!)
                            .ToList();

        _porPantalla = _permisos
            .GroupBy(p => p.Pantalla, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    private string? Claim(string tipo) => _usuario?.FindFirst(tipo)?.Value;

}
