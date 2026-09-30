using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace SuvesaPosSitioAplicacion.Security;

/// <summary>
/// Guarda el ticket de autenticacion en servidor y deja en la cookie solo una llave.
///
/// Motivo: el ticket lleva el token del API y un permiso por cada una de las ~82
/// pantallas. Metido en la cookie superaria los 4 KB y el navegador la partiria en
/// trozos que viajan en cada peticion. Aparte, el token del API no tiene por que
/// salir del servidor ni siquiera cifrado.
///
/// LIMITE CONOCIDO: el almacen por defecto es memoria del proceso, asi que las
/// sesiones se pierden al reiniciar la aplicacion y no se comparten entre instancias.
/// Para varias instancias basta cambiar el IDistributedCache por Redis o SQL en
/// Program.cs; esta clase no cambia.
/// </summary>
public sealed class AlmacenTickets : ITicketStore
{
    private const string Prefijo = "seepos-sesion-";

    private readonly IDistributedCache _cache;
    private readonly RegistroSesiones _registro;

    public AlmacenTickets(IDistributedCache cache, RegistroSesiones registro)
    {
        _cache = cache;
        _registro = registro;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var llave = Prefijo + Guid.NewGuid().ToString("N");
        await RenewAsync(llave, ticket);
        var anterior = Registrar(llave, ticket);
        if (!string.IsNullOrEmpty(anterior))
            await _cache.RemoveAsync(anterior);
        return llave;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var opciones = new DistributedCacheEntryOptions();

        // Una expiracion ya pasada haria que el ticket se desalojara al instante y
        // la sesion se perdiera entre dos peticiones, con un 401 como unico sintoma.
        // Aqui no se acepta: si no esta en el futuro, manda la ventana deslizante.
        var expira = ticket.Properties.ExpiresUtc;

        if (expira.HasValue && expira.Value > DateTimeOffset.UtcNow)
        {
            opciones.SetAbsoluteExpiration(expira.Value);
        }
        else
        {
            opciones.SetSlidingExpiration(TimeSpan.FromHours(12));
        }

        return _cache.SetAsync(key, TicketSerializer.Default.Serialize(ticket), opciones);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var bytes = await _cache.GetAsync(key);
        return bytes is null ? null : TicketSerializer.Default.Deserialize(bytes);
    }

    public Task RemoveAsync(string key)
    {
        _registro.QuitarPorLlave(key);
        return _cache.RemoveAsync(key);
    }

    public async Task<IReadOnlyList<SesionConectada>> SesionesVigentesAsync()
    {
        var vivas = new List<SesionConectada>();
        foreach (var sesion in _registro.Listar())
        {
            if (await _cache.GetAsync(sesion.Llave) is null)
                _registro.Quitar(sesion.Id);
            else
                vivas.Add(sesion);
        }
        return vivas;
    }

    public async Task<bool> CerrarSesionAsync(string id)
    {
        var llave = _registro.Quitar(id);
        if (string.IsNullOrEmpty(llave))
            return false;
        await _cache.RemoveAsync(llave);
        return true;
    }

    private string? Registrar(string llave, AuthenticationTicket ticket)
    {
        var id = ticket.Principal.FindFirst(ClaimsSeePos.SesionId)?.Value;
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var usuario = ticket.Principal.Identity?.Name ?? "";
        var nombre = ticket.Principal.FindFirst(ClaimsSeePos.NombreUsuario)?.Value;
        var inicioTexto = ticket.Principal.FindFirst(ClaimsSeePos.InicioSesion)?.Value;
        var inicio = DateTime.TryParse(inicioTexto, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var leida)
            ? leida
            : DateTime.Now;

        return _registro.Registrar(new SesionConectada
        {
            Id = id,
            Llave = llave,
            Usuario = usuario,
            Nombre = string.IsNullOrWhiteSpace(nombre) ? usuario : nombre,
            DireccionIp = ticket.Principal.FindFirst(ClaimsSeePos.DireccionIp)?.Value,
            UserAgent = ticket.Principal.FindFirst(ClaimsSeePos.AgenteSesion)?.Value,
            Inicio = inicio,
            UltimaActividad = inicio,
        });
    }
}
