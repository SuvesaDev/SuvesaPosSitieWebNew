using System.Globalization;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Correo;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IEnviosCorreo" />
public sealed class EnviosCorreo : ProxyBase, IEnviosCorreo
{
    private readonly HttpClient _api;

    public EnviosCorreo(IHttpClientFactory factory, IContextoSesion sesion, ILogger<EnviosCorreo> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<PaginaEnviosCorreoDTO>> Listar(
        string? estado, int? idEmisor, DateTime? desde, DateTime? hasta, string? texto, int pagina, int tamano)
    {
        var q = new List<string> { $"pagina={pagina}", $"tamano={tamano}" };
        if (!string.IsNullOrWhiteSpace(estado)) q.Add($"estado={Uri.EscapeDataString(estado)}");
        if (idEmisor is { } e) q.Add($"idEmisor={e}");
        if (desde is { } d) q.Add($"desde={Uri.EscapeDataString(d.ToString("o", CultureInfo.InvariantCulture))}");
        if (hasta is { } h) q.Add($"hasta={Uri.EscapeDataString(h.ToString("o", CultureInfo.InvariantCulture))}");
        if (!string.IsNullOrWhiteSpace(texto)) q.Add($"texto={Uri.EscapeDataString(texto)}");

        return Ejecutar(async () => await LecturaEnvelope.Leer<PaginaEnviosCorreoDTO>(
            await _api.GetAsync($"api/envios-correo/comprobantes?{string.Join("&", q)}")), "consultar los envíos de correo");
    }

    public Task<ResponseGeneric<bool>> Reenviar(string clave)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.PostAsync($"api/envios-correo/comprobantes/{Uri.EscapeDataString(clave)}/reenviar", null)),
            "reencolar el envío");
}
