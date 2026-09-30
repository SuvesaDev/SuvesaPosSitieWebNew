using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// En Blazor Server un <c>DelegatingHandler</c> de HttpClient corre en un ámbito de
/// DI distinto al del circuito, así que no puede resolver servicios con estado del
/// circuito (como <see cref="IEstadoOcupado"/>). Este accesor publica el
/// <see cref="IServiceProvider"/> del circuito en un <see cref="AsyncLocal{T}"/>
/// durante cada actividad entrante (render/evento) para que el handler lo alcance.
/// Patrón oficial: "Access server-side Blazor services from a different DI scope".
/// </summary>
public sealed class AccesorServiciosCircuito
{
    private static readonly AsyncLocal<IServiceProvider?> _actual = new();

    public IServiceProvider? Servicios
    {
        get => _actual.Value;
        set => _actual.Value = value;
    }
}

/// <summary>Publica los servicios del circuito en <see cref="AccesorServiciosCircuito"/>.</summary>
public sealed class CircuitoServiciosHandler(AccesorServiciosCircuito accesor, IServiceProvider servicios, RegistroSesiones registro) : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> siguiente)
        => async contexto =>
        {
            accesor.Servicios = servicios;
            var id = SesionActual();
            if (id is not null && registro.FueCerrada(id))
            {
                if (servicios.GetService(typeof(NavigationManager)) is NavigationManager navegacion)
                    navegacion.NavigateTo("/cuenta/salir", forceLoad: true);
            }
            else if (id is not null)
            {
                registro.Tocar(id);
            }
            await siguiente(contexto);
        };

    public override Task OnCircuitOpenedAsync(Circuit circuito, CancellationToken cancellationToken)
    {
        var id = SesionActual();
        if (id is not null)
            registro.Tocar(id);
        return base.OnCircuitOpenedAsync(circuito, cancellationToken);
    }

    private string? SesionActual()
    {
        if (servicios.GetService(typeof(AuthenticationStateProvider)) is not AuthenticationStateProvider proveedor)
            return null;
        var estado = proveedor.GetAuthenticationStateAsync();
        if (!estado.IsCompletedSuccessfully)
            return null;
        return estado.Result.User.FindFirst(ClaimsSeePos.SesionId)?.Value;
    }
}

/// <summary>Última hora local leída del navegador, visible para el cliente HTTP del circuito.</summary>
public static class MarcaEquipo
{
    private static readonly System.Threading.AsyncLocal<DateTime?> _actual = new();
    public static DateTime? Actual
    {
        get => _actual.Value;
        set => _actual.Value = value;
    }
}

/// <summary>IP y navegador de quien está ingresando, para que el API no vea al servidor Blazor.</summary>
public static class OrigenEquipo
{
    private static readonly System.Threading.AsyncLocal<string?> _ip = new();
    private static readonly System.Threading.AsyncLocal<string?> _agente = new();
    public static string? DireccionIp { get => _ip.Value; set => _ip.Value = value; }
    public static string? Agente { get => _agente.Value; set => _agente.Value = value; }
}
