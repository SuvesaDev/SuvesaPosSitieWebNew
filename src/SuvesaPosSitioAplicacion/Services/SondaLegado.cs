using Microsoft.Extensions.Caching.Memory;

namespace SuvesaPosSitioAplicacion.Services;

/// <inheritdoc cref="ISondaLegado" />
public sealed class SondaLegado : ISondaLegado
{
    private const string Llave = "legado-disponible";

    private readonly IHttpClientFactory _fabrica;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;
    private readonly ILogger<SondaLegado> _log;

    public SondaLegado(
        IHttpClientFactory fabrica,
        IMemoryCache cache,
        IConfiguration config,
        ILogger<SondaLegado> log)
    {
        _fabrica = fabrica;
        _cache = cache;
        _config = config;
        _log = log;
    }

    public async Task<bool> EstaDisponibleAsync(CancellationToken token = default)
    {
        var url = _config["SeePos:LegacySpaUrl"];

        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        // Se cachea corto: si alguien levanta la SPA, la aplicacion se entera pronto,
        // pero no se sondea en cada render de cada pestana.
        if (_cache.TryGetValue<bool>(Llave, out var guardado))
        {
            return guardado;
        }

        bool disponible;

        try
        {
            using var http = _fabrica.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(3);

            using var r = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
            disponible = r.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _log.LogDebug(ex, "La SPA legada no responde en {Url}", url);
            disponible = false;
        }

        _cache.Set(Llave, disponible, TimeSpan.FromSeconds(disponible ? 60 : 15));
        return disponible;
    }
}
