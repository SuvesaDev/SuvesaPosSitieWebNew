using Microsoft.JSInterop;

namespace SuvesaPosSitioAplicacion.Services;

public sealed class RelojEquipo(IJSRuntime js) : IRelojEquipo, IAsyncDisposable
{
    private IJSObjectReference? _modulo;

    public async Task<DateTime> AhoraAsync(CancellationToken cancellationToken = default)
    {
        _modulo ??= await js.InvokeAsync<IJSObjectReference>("import", cancellationToken, "./js/tema.js");
        var iso = await _modulo.InvokeAsync<string>("obtenerFechaHoraEquipo", cancellationToken);
        return DateTime.SpecifyKind(DateTime.Parse(iso, null, System.Globalization.DateTimeStyles.RoundtripKind).ToLocalTime(), DateTimeKind.Unspecified);
    }

    public async Task<DateTime> HoyAsync(CancellationToken cancellationToken = default)
        => (await AhoraAsync(cancellationToken)).Date;

    public async ValueTask DisposeAsync()
    {
        if (_modulo is not null) await _modulo.DisposeAsync();
    }
}
