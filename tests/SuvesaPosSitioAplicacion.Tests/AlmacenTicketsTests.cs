using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Tests;

/// <summary>
/// El almacen guarda la sesion. Si un ticket se desaloja antes de tiempo, el usuario
/// queda anonimo entre dos peticiones y el unico sintoma es un 401 sin explicacion.
/// </summary>
public class AlmacenTicketsTests
{
    private static AlmacenTickets Crear() =>
        new(new MemoryDistributedCache(
            Options.Create(new MemoryDistributedCacheOptions())),
            new RegistroSesiones());

    private static AuthenticationTicket Ticket(DateTimeOffset? expira)
    {
        var identidad = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "prueba") },
            CookieAuthenticationDefaults.AuthenticationScheme);

        var propiedades = new AuthenticationProperties { ExpiresUtc = expira };

        return new AuthenticationTicket(
            new ClaimsPrincipal(identidad),
            propiedades,
            CookieAuthenticationDefaults.AuthenticationScheme);
    }

    [Fact]
    public async Task ConExpiracionEnElFuturo_ElTicketSeRecupera()
    {
        var almacen = Crear();

        var llave = await almacen.StoreAsync(Ticket(DateTimeOffset.UtcNow.AddHours(8)));
        var vuelta = await almacen.RetrieveAsync(llave);

        Assert.NotNull(vuelta);
        Assert.Equal("prueba", vuelta!.Principal.Identity?.Name);
    }

    [Fact]
    public async Task SinExpiracion_ElTicketSeRecupera()
    {
        var almacen = Crear();

        var llave = await almacen.StoreAsync(Ticket(null));

        Assert.NotNull(await almacen.RetrieveAsync(llave));
    }

    [Fact]
    public async Task ConExpiracionYaPasada_ElTicketSIGUEDisponible()
    {
        // Este es el caso que rompia el inicio de sesion. Si el API devuelve una
        // expiracion vacia o vencida y se pasa tal cual al almacen, el ticket se
        // desaloja al instante y la sesion no sobrevive a la redireccion.
        var almacen = Crear();

        var llave = await almacen.StoreAsync(Ticket(DateTimeOffset.UtcNow.AddHours(-1)));

        Assert.NotNull(await almacen.RetrieveAsync(llave));
    }

    [Fact]
    public async Task ConFechaPorDefecto_ElTicketSIGUEDisponible()
    {
        var almacen = Crear();

        var llave = await almacen.StoreAsync(Ticket(default(DateTimeOffset)));

        Assert.NotNull(await almacen.RetrieveAsync(llave));
    }

    [Fact]
    public async Task AlBorrar_ElTicketDesaparece()
    {
        var almacen = Crear();

        var llave = await almacen.StoreAsync(Ticket(DateTimeOffset.UtcNow.AddHours(1)));
        await almacen.RemoveAsync(llave);

        Assert.Null(await almacen.RetrieveAsync(llave));
    }

    [Fact]
    public async Task CerrarSesion_InvalidaElTicketYLaQuitaDeLaLista()
    {
        var almacen = Crear();
        var llave = await almacen.StoreAsync(TicketConSesion("ana", "sesion-ana"));

        Assert.Single(await almacen.SesionesVigentesAsync());

        Assert.True(await almacen.CerrarSesionAsync("sesion-ana"));

        Assert.Null(await almacen.RetrieveAsync(llave));
        Assert.Empty(await almacen.SesionesVigentesAsync());
    }

    private static AuthenticationTicket TicketConSesion(string usuario, string id)
    {
        var identidad = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, usuario),
                new Claim(ClaimsSeePos.SesionId, id),
                new Claim(ClaimsSeePos.NombreUsuario, usuario),
                new Claim(ClaimsSeePos.InicioSesion, "2026-09-29T18:00:00"),
            },
            CookieAuthenticationDefaults.AuthenticationScheme);

        return new AuthenticationTicket(
            new ClaimsPrincipal(identidad),
            new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) },
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
