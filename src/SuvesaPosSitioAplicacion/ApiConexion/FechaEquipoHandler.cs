using System.Net.Http;
using SuvesaPosSitioAplicacion.Services;

namespace SuvesaPosSitioAplicacion.ApiConexion;

/// <summary>Envía la hora local del navegador para que la bitácora no use el reloj del servidor.</summary>
public sealed class FechaEquipoHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage solicitud, CancellationToken ct)
    {
        if (MarcaEquipo.Actual is { } marca)
            solicitud.Headers.TryAddWithoutValidation("X-Fecha-Equipo", marca.ToString("yyyy-MM-ddTHH:mm:ss"));
        if (!string.IsNullOrWhiteSpace(OrigenEquipo.DireccionIp))
            solicitud.Headers.TryAddWithoutValidation("X-Forwarded-For", OrigenEquipo.DireccionIp);
        if (!string.IsNullOrWhiteSpace(OrigenEquipo.Agente))
            solicitud.Headers.TryAddWithoutValidation("User-Agent", OrigenEquipo.Agente);
        return base.SendAsync(solicitud, ct);
    }
}
