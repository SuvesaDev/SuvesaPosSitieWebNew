namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Puente entre la pantalla de Alertas y la campana del encabezado, dentro del mismo
/// circuito de Blazor Server. Cuando se marca una alerta como leída (o llega una nueva),
/// la pantalla llama <see cref="NotificarAsync"/> y la campana refresca su contador al
/// instante, sin esperar al sondeo periódico.
/// </summary>
public interface IAvisadorAlertas
{
    /// <summary>Se dispara cuando el estado de las alertas pudo cambiar.</summary>
    event Func<Task>? Cambio;

    Task NotificarAsync();
}

/// <inheritdoc cref="IAvisadorAlertas" />
public sealed class AvisadorAlertas : IAvisadorAlertas
{
    public event Func<Task>? Cambio;

    public async Task NotificarAsync()
    {
        var handler = Cambio;
        if (handler is null) return;
        foreach (var d in handler.GetInvocationList().Cast<Func<Task>>())
        {
            try { await d(); } catch { /* un suscriptor caído no debe frenar a los demás */ }
        }
    }
}
