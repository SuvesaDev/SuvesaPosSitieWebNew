using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SuvesaPosSitioAplicacion.Class;

// ContextoSesion resuelve la "clave de pantalla" a codigo de funcion via
// MenuSeePos.ResolverCodigo: las Views que aun pasan el titulo siguen funcionando.

namespace SuvesaPosSitioAplicacion.Security;

/// <inheritdoc cref="IContextoSesion" />
public sealed class ContextoSesion : IContextoSesion
{
    private readonly AuthenticationStateProvider _estado;
    private readonly bool _verLoNoGobernado;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<ContextoSesion> _log;

    private ClaimsPrincipal? _usuario;
    private List<PermisoFuncion> _permisos = new();
    private Dictionary<string, PermisoFuncion> _porFuncion = new(StringComparer.OrdinalIgnoreCase);

    public ContextoSesion(
        AuthenticationStateProvider estado,
        IHttpContextAccessor http,
        IConfiguration config,
        ILogger<ContextoSesion> log)
    {
        _estado = estado;
        _http = http;
        _log = log;

        // Por defecto TRUE = paridad con el sistema actual (el menu no se filtra).
        // Con el catalogo generado desde el mismo arbol que el menu, ya se puede poner
        // en false sin esconder pantallas legitimas.
        _verLoNoGobernado = config.GetValue("SeePos:VerPantallasNoGobernadas", true);
    }

    public bool Autenticado => _usuario?.Identity?.IsAuthenticated ?? false;

    public string? Token => Claim(ClaimsSeePos.Token);
    public string? Usuario => _usuario?.Identity?.Name;

    public string? NombreUsuario
    {
        get
        {
            var n = Claim(ClaimsSeePos.NombreUsuario);
            return string.IsNullOrWhiteSpace(n) ? Usuario : n;
        }
    }

    public string? CorreoUsuario => Claim(ClaimsSeePos.CorreoUsuario);

    public string InicialesUsuario
    {
        get
        {
            var explicito = Claim(ClaimsSeePos.InicialesUsuario);
            if (!string.IsNullOrWhiteSpace(explicito)) return explicito.Trim().ToUpperInvariant();

            var fuente = NombreUsuario ?? Usuario ?? "";
            var palabras = fuente.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (palabras.Length >= 2) return (palabras[0][..1] + palabras[1][..1]).ToUpperInvariant();
            if (palabras.Length == 1 && palabras[0].Length >= 2) return palabras[0][..2].ToUpperInvariant();
            return palabras.Length == 1 ? palabras[0].ToUpperInvariant() : "?";
        }
    }

    public bool EsSuperAdministrador => Claim(ClaimsSeePos.EsSuperAdministrador) == bool.TrueString;
    public bool EsAdministrador => EsSuperAdministrador;
    public string? PerfilCodigo => Claim(ClaimsSeePos.PerfilCodigo);

    public bool EsCostaPets => Claim(ClaimsSeePos.CostaPets) == bool.TrueString;
    public bool EsAgenteCostaPets => Claim(ClaimsSeePos.AgenteCostaPets) == bool.TrueString;
    public bool PermitirExistenciaNegativa => Claim(ClaimsSeePos.PermiteExistenciaNegativa) == bool.TrueString;

    public int IdSucursal => int.TryParse(Claim(ClaimsSeePos.IdSucursal), out var id) ? id : 0;
    public string? NombreSucursal => Claim(ClaimsSeePos.NombreSucursal);
    public bool TieneSucursal => IdSucursal > 0;

    public IReadOnlyCollection<string> Menus =>
        _permisos.Select(p => p.ModuloCodigo)
                 .Where(m => !string.IsNullOrWhiteSpace(m))
                 .Distinct(StringComparer.OrdinalIgnoreCase)
                 .ToList();

    public IReadOnlyCollection<PermisoFuncion> Permisos => _permisos;

    public bool PuedeVer(string funcionCodigo) => Puede(funcionCodigo, AccionPantalla.Ver);

    public bool EstaGobernada(string funcionCodigo)
        => _porFuncion.ContainsKey(MenuSeePos.ResolverCodigo(funcionCodigo));

    public bool Puede(string funcionCodigo, AccionPantalla accion)
    {
        if (!Autenticado)
        {
            return false;
        }

        // SUPER_ADMIN entra a todo y no pasa por rol.
        if (EsSuperAdministrador)
        {
            return true;
        }

        var codigo = MenuSeePos.ResolverCodigo(funcionCodigo);

        if (_porFuncion.TryGetValue(codigo, out var permiso))
        {
            return permiso.Permite(accion);
        }

        // Funcion no mencionada por el rol: no es lo mismo que denegada mientras el
        // catalogo del API no este completo. Quien autoriza de verdad es el API.
        return _verLoNoGobernado;
    }

    public async Task CargarAsync()
    {
        if (_usuario is not null)
        {
            return;
        }

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
                // Fuera del ambito de DI de un componente Razor no hay estado que leer.
                // Pasa durante el propio inicio de sesion. Se trata como "sin sesion".
                _usuario = new ClaimsPrincipal(new ClaimsIdentity());
                return;
            }
        }

        if (_usuario.Identity?.IsAuthenticated == true
            && string.IsNullOrWhiteSpace(_usuario.FindFirst(ClaimsSeePos.Token)?.Value))
        {
            _log.LogWarning(
                "La sesion de {Usuario} no lleva token del API. Las llamadas respondera 401.",
                _usuario.Identity.Name);
        }

        _permisos = _usuario.FindAll(ClaimsSeePos.Permiso)
                            .Select(c => PermisoFuncion.DesdeClaim(c.Value))
                            .Where(p => p is not null)
                            .Select(p => p!)
                            .ToList();

        _porFuncion = _permisos
            .GroupBy(p => p.FuncionCodigo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    private string? Claim(string tipo) => _usuario?.FindFirst(tipo)?.Value;
}
