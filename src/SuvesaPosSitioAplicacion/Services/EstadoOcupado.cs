namespace SuvesaPosSitioAplicacion.Services;

/// <summary>
/// Cuenta cuántas llamadas al API hay en vuelo en el circuito. El layout muestra
/// una barra de carga arriba mientras <see cref="Ocupado"/> sea true, así el
/// usuario ve que una consulta/espera está en curso y no cree que el botón "no
/// hizo nada". El <see cref="OcupadoHandler"/> (DelegatingHandler) llama a
/// <see cref="Rastrear"/> por cada request; las pantallas pueden usarlo también
/// para esperas que no son HTTP.
/// </summary>
public interface IEstadoOcupado
{
    /// <summary>Hay al menos una operación en curso.</summary>
    bool Ocupado { get; }

    /// <summary>Se dispara en cada transición ocupado ↔ libre.</summary>
    event Func<Task>? Cambio;

    /// <summary>Marca una operación en curso; <c>Dispose()</c> la da por terminada.</summary>
    IDisposable Rastrear();
}

/// <inheritdoc cref="IEstadoOcupado" />
public sealed class EstadoOcupado : IEstadoOcupado
{
    private int _enVuelo;

    public bool Ocupado => Volatile.Read(ref _enVuelo) > 0;

    public event Func<Task>? Cambio;

    public IDisposable Rastrear()
    {
        if (Interlocked.Increment(ref _enVuelo) == 1) _ = Notificar();
        return new Token(this);
    }

    private void Soltar()
    {
        if (Interlocked.Decrement(ref _enVuelo) == 0) _ = Notificar();
    }

    private async Task Notificar()
    {
        var handler = Cambio;
        if (handler is null) return;
        foreach (var d in handler.GetInvocationList().Cast<Func<Task>>())
        {
            try { await d(); } catch { /* un suscriptor caído no frena a los demás */ }
        }
    }

    private sealed class Token(EstadoOcupado dueno) : IDisposable
    {
        private int _liberado;
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _liberado, 1) == 0) dueno.Soltar();
        }
    }
}
