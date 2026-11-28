using Microsoft.JSInterop;

namespace SuvesaPosSitioAplicacion.Services;

public sealed class RelojEquipo(IJSRuntime js) : IRelojEquipo, IAsyncDisposable
{
    private IJSObjectReference? _modulo;

    public async Task<DateTime> AhoraAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _modulo ??= await js.InvokeAsync<IJSObjectReference>("import", cancellationToken, "./js/tema.js");
            await PublicarDesdeModuloAsync(cancellationToken);
            var iso = await _modulo.InvokeAsync<string>("obtenerFechaHoraEquipo", cancellationToken);
            return ACostaRica(DateTimeOffset.Parse(iso, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind));
        }
        catch (InvalidOperationException)
        {
            // Prerenderizado estático: todavía no hay circuito interactivo para hacer
            // interop con JS. OnInitializedAsync se vuelve a ejecutar cuando el
            // circuito conecta, y ahí sí se obtiene la hora real del equipo.
            return ACostaRica(DateTimeOffset.UtcNow);
        }
    }

    /// <summary>Costa Rica es UTC-6 todo el año. NO se usa ToLocalTime(): en Blazor Server correría con la zona del
    /// SERVIDOR (p. ej. Pacifico, UTC-8) y la hora de emision saldria 2 h atrasada.</summary>
    internal static DateTime ACostaRica(DateTimeOffset instante)
        => DateTime.SpecifyKind(instante.UtcDateTime.AddHours(-6), DateTimeKind.Unspecified);

    public async Task<DateTime> HoyAsync(CancellationToken cancellationToken = default)
        => (await AhoraAsync(cancellationToken)).Date;

    public async Task PublicarMarcaAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _modulo ??= await js.InvokeAsync<IJSObjectReference>("import", cancellationToken, "./js/tema.js");
            await PublicarDesdeModuloAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSException)
        {
        }
    }

    private async Task PublicarDesdeModuloAsync(CancellationToken cancellationToken)
    {
        var utc = await _modulo!.InvokeAsync<string>("obtenerFechaHoraEquipo", cancellationToken);
        if (DateTimeOffset.TryParse(utc, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out var instante))
            MarcaEquipo.ActualUtc = instante;
        var iso = await _modulo!.InvokeAsync<string>("obtenerMarcaEquipo", cancellationToken);
        if (DateTime.TryParse(iso, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var marca))
            MarcaEquipo.Actual = DateTime.SpecifyKind(marca, DateTimeKind.Unspecified);
    }

    public async ValueTask DisposeAsync()
    {
        if (_modulo is not null) await _modulo.DisposeAsync();
    }
}
