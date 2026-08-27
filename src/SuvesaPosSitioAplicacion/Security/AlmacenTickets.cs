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

    public AlmacenTickets(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var llave = Prefijo + Guid.NewGuid().ToString("N");
        await RenewAsync(llave, ticket);
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

    public Task RemoveAsync(string key) => _cache.RemoveAsync(key);
}
