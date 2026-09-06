using Microsoft.AspNetCore.Components.Server.Circuits;

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
public sealed class CircuitoServiciosHandler(AccesorServiciosCircuito accesor, IServiceProvider servicios) : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> siguiente)
        => async contexto =>
        {
            accesor.Servicios = servicios;
            await siguiente(contexto);
        };
}
