using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
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

    private static async Task FirmarAsync(HttpContext contexto, IList<Claim> claims, DateTime expiracion)
    {
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await contexto.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad),
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = new DateTimeOffset(expiracion.ToUniversalTime())
            });
    }

    /// <summary>
    /// Aplana lo que devuelve el API en claims. Todo esto vive en el ticket, que se
    /// guarda en servidor; la cookie del navegador solo lleva la llave del ticket.
    /// </summary>
    private static List<Claim> ConstruirClaims(Autenticacion auth)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, auth.Usuario ?? string.Empty),
            new(ClaimsSeePos.Token, auth.Token!),
            new(ClaimsSeePos.Expiracion, auth.Expiracion.ToString("O")),
            new(ClaimsSeePos.Administrador, auth.Administrador.ToString()),
            new(ClaimsSeePos.CostaPets, auth.CostaPets.ToString()),
            new(ClaimsSeePos.AgenteCostaPets, auth.AgenteCostaPets.ToString()),
            new(ClaimsSeePos.AceptaConsignacion, auth.AceptaConsignacion.ToString())
        };

        if (auth.Rol is not null)
        {
            claims.Add(new Claim(ClaimsSeePos.IdRol, auth.Rol.IdRol.ToString()));
            claims.Add(new Claim(ClaimsSeePos.NombreRol, auth.Rol.NombreRol ?? string.Empty));
            claims.Add(new Claim(ClaimTypes.Role, auth.Rol.NombreRol ?? string.Empty));

            foreach (var p in auth.Rol.Permisos ?? Array.Empty<PermisosDTO>())
            {
                if (string.IsNullOrWhiteSpace(p.NombrePantalla))
                {
                    continue;
                }

                var permiso = new PermisoPantalla(
                    Menu: p.Menu ?? string.Empty,
                    Pantalla: p.NombrePantalla,
                    Ver: p.Acciones?.Ver ?? false,
                    Crear: p.Acciones?.Crear ?? false,
                    Modificar: p.Acciones?.Modificar ?? false,
                    Borrar: p.Acciones?.Borrar ?? false);

                claims.Add(new Claim(ClaimsSeePos.Permiso, permiso.AClaim()));
            }
        }

        return claims;
    }
}
