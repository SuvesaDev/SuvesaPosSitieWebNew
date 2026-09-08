using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Seguridad;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Services;

/// <inheritdoc cref="IServicioAutenticacion" />
public sealed class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly ISeguridad _seguridad;
    private readonly ILogger<ServicioAutenticacion> _log;

    public ServicioAutenticacion(ISeguridad seguridad, ILogger<ServicioAutenticacion> log)
    {
        _seguridad = seguridad;
        _log = log;
    }

    public async Task<Response> IngresarAsync(HttpContext contexto, string usuario, string password)
    {
        var r = await _seguridad.Login(usuario, password);

        if (!r.EsCorrecta || r.Responses is null)
        {
            return new Response(r.Excepcion ?? "No se pudo validar el usuario.");
        }

        var auth = r.Responses;

        if (string.IsNullOrWhiteSpace(auth.Token))
        {
            return new Response("El API no devolvio un token.");
        }

        var claims = ConstruirClaims(auth);

        await FirmarAsync(contexto, claims, auth.Expiracion);

        _log.LogInformation("Sesion iniciada para {Usuario}", auth.Usuario);
        return new Response();
    }

    public async Task<Response> EstablecerSucursalAsync(HttpContext contexto, SucursalDTO sucursal)
    {
        var actual = contexto.User;

        if (actual.Identity?.IsAuthenticated != true)
        {
            return new Response("La sesion expiro. Vuelva a ingresar.");
        }

        // Se conservan todos los claims menos los de sucursal, que se reemplazan.
        var claims = actual.Claims
            .Where(c => c.Type != ClaimsSeePos.IdSucursal && c.Type != ClaimsSeePos.NombreSucursal)
            .ToList();

        claims.Add(new Claim(ClaimsSeePos.IdSucursal, sucursal.Id.ToString()));
        claims.Add(new Claim(ClaimsSeePos.NombreSucursal, sucursal.Alias ?? sucursal.NombreComercial ?? string.Empty));

        var expiracion = DateTime.TryParse(
            actual.FindFirst(ClaimsSeePos.Expiracion)?.Value,
            out var e) ? e : DateTime.UtcNow.AddHours(12);

        await FirmarAsync(contexto, claims, expiracion);
        return new Response();
    }

    public Task SalirAsync(HttpContext contexto)
        => contexto.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    private async Task FirmarAsync(HttpContext contexto, IList<Claim> claims, DateTime expiracion)
    {
        // Si la respuesta ya empezo a escribirse, SignInAsync no puede poner la
        // cabecera Set-Cookie y la sesion se pierde sin dar error. Es el fallo
        // clasico de firmar dentro del render de un componente.
        var yaEmpezo = contexto.Response.HasStarted;
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await contexto.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad),
            new AuthenticationProperties
            {
                IsPersistent = false,

                // Solo se fija si el API mando una expiracion util. Si llega vacia o
                // ya pasada, se deja nula y manda el ExpireTimeSpan de la cookie.
                // Ponerla en el pasado hacia que el ticket se desalojara al instante
                // del almacen: la sesion se perdia entre una peticion y la siguiente,
                // y el sintoma era un 401 sin explicacion.
                ExpiresUtc = ExpiracionUtil(expiracion)
            });

        _log.LogInformation(
            "Firma de sesion: expiracion del API = {Expiracion:O}, aplicada = {Aplicada}, " +
            "respuesta ya iniciada = {YaEmpezo}, Set-Cookie presente = {HayCookie}",
            expiracion,
            ExpiracionUtil(expiracion),
            yaEmpezo,
            contexto.Response.Headers.SetCookie.Count > 0);
    }

    /// <summary>
    /// La expiracion del API solo se usa si de verdad esta en el futuro. Se exige un
    /// margen de un minuto para no aceptar una que caduque mientras se responde.
    /// </summary>
    private static DateTimeOffset? ExpiracionUtil(DateTime expiracion)
    {
        if (expiracion == default)
        {
            return null;
        }

        var utc = expiracion.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(expiracion, DateTimeKind.Utc)
            : expiracion.ToUniversalTime();

        return utc > DateTime.UtcNow.AddMinutes(1) ? new DateTimeOffset(utc) : null;
    }

    /// <summary>
    /// Aplana lo que devuelve el API en claims. Todo esto vive en el ticket, que se
    /// guarda en servidor; la cookie del navegador solo lleva la llave del ticket.
    /// </summary>
    private static List<Claim> ConstruirClaims(Autenticacion auth)
    {
        var esSuper = auth.Perfil?.EsSuperAdministracion ?? auth.Administrador;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, auth.Usuario ?? string.Empty),
            new(ClaimsSeePos.NombreUsuario, auth.Nombre ?? string.Empty),
            new(ClaimsSeePos.CorreoUsuario, auth.Email ?? string.Empty),
            new(ClaimsSeePos.InicialesUsuario, auth.Iniciales ?? string.Empty),
            new(ClaimsSeePos.Token, auth.Token!),
            new(ClaimsSeePos.Expiracion, auth.Expiracion.ToString("O")),
            new(ClaimsSeePos.EsSuperAdministrador, esSuper.ToString()),
            new(ClaimsSeePos.PerfilCodigo, auth.Perfil?.Codigo ?? string.Empty),
            new(ClaimsSeePos.CostaPets, (auth.Perfil?.CostaPets ?? auth.CostaPets).ToString()),
            new(ClaimsSeePos.AgenteCostaPets, (auth.Perfil?.AgenteCostaPets ?? auth.AgenteCostaPets).ToString()),
            new(ClaimsSeePos.AceptaConsignacion, (auth.Perfil?.AceptaConsignacion ?? auth.AceptaConsignacion).ToString()),
            new(ClaimsSeePos.PermiteExistenciaNegativa, (auth.Perfil?.PermiteExistenciaNegativa ?? auth.PermiteExistenciaNegativa).ToString())
        };

        // Rol (informativo). auth.Rol es el contrato viejo; los campos planos IdRol /
        // NombreRol del contrato nuevo se leen si estan.
        if (auth.Rol is not null)
        {
            claims.Add(new Claim(ClaimsSeePos.IdRol, auth.Rol.IdRol.ToString()));
            claims.Add(new Claim(ClaimsSeePos.NombreRol, auth.Rol.NombreRol ?? string.Empty));
            if (!string.IsNullOrWhiteSpace(auth.Rol.NombreRol))
            {
                claims.Add(new Claim(ClaimTypes.Role, auth.Rol.NombreRol));
            }
        }

        // Permisos aplanados del rol (contrato nuevo). Vacio si es SUPER_ADMIN.
        foreach (var p in auth.Permisos ?? new List<PermisoLoginDTO>())
        {
            if (string.IsNullOrWhiteSpace(p.FuncionCodigo))
            {
                continue;
            }

            var acciones = (p.Acciones ?? new List<string>())
                .Select(a => a.Trim().ToUpperInvariant())
                .Where(a => a.Length > 0)
                .ToHashSet();

            var permiso = new PermisoFuncion(
                ModuloCodigo: p.ModuloCodigo ?? string.Empty,
                FuncionCodigo: p.FuncionCodigo,
                Acciones: acciones);

            claims.Add(new Claim(ClaimsSeePos.Permiso, permiso.AClaim()));
        }

        return claims;
    }
}
