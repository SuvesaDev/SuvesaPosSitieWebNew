using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Compras;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ITramitesCobro" />
public sealed class TramitesCobro : ProxyBase, ITramitesCobro
{
    private readonly HttpClient _api;

    public TramitesCobro(IHttpClientFactory factory, IContextoSesion sesion, ILogger<TramitesCobro> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IReadOnlyList<FacturaTramiteCobroWebDTO>>> Candidatas(long idCliente)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<FacturaTramiteCobroWebDTO>>(
            await _api.GetAsync($"api/tramites-cobro/candidatas?idCliente={idCliente}")), "consultar las facturas pendientes");

    public Task<ResponseGeneric<TramiteCobroWebDTO>> Crear(CrearTramiteCobroWebDTO cmd)
        => Ejecutar(async () => await LecturaEnvelope.Leer<TramiteCobroWebDTO>(
            await _api.PostAsJsonAsync("api/tramites-cobro", cmd, LecturaEnvelope.Json)), "crear la boleta de trámite de cobro");

    public Task<ResponseGeneric<IReadOnlyList<TramiteCobroWebDTO>>> Listar(
        long? idCliente = null, bool incluirAnuladas = false,
        DateTime? desde = null, DateTime? hasta = null, long? consecutivo = null, int limite = 200)
        => Ejecutar(async () =>
        {
            var q = new List<string> { $"incluirAnuladas={incluirAnuladas.ToString().ToLowerInvariant()}", $"limite={limite}" };
            if (idCliente is { } c) q.Add($"idCliente={c}");
            if (desde is { } d) q.Add($"desde={d:yyyy-MM-dd}");
            if (hasta is { } h) q.Add($"hasta={h:yyyy-MM-dd}");
            if (consecutivo is { } n) q.Add($"consecutivo={n}");
            return await LecturaEnvelope.Leer<IReadOnlyList<TramiteCobroWebDTO>>(
                await _api.GetAsync("api/tramites-cobro?" + string.Join("&", q)));
        }, "consultar las boletas de trámite de cobro");

    public Task<ResponseGeneric<TramiteCobroWebDTO>> Obtener(long id)
        => Ejecutar(async () => await LecturaEnvelope.Leer<TramiteCobroWebDTO>(
            await _api.GetAsync($"api/tramites-cobro/{id}")), "consultar la boleta de trámite de cobro");

    public Task<ResponseGeneric<TramiteCobroWebDTO>> Anular(long id, string? motivo)
        => Ejecutar(async () => await LecturaEnvelope.Leer<TramiteCobroWebDTO>(
            await _api.PostAsJsonAsync($"api/tramites-cobro/{id}/anular", new { Motivo = motivo }, LecturaEnvelope.Json)),
            "anular la boleta de trámite de cobro");

    public Task<ResponseGeneric<ResultadoEnvioTramiteCobroWebDTO>> EnviarCorreo(long id, string? destino)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ResultadoEnvioTramiteCobroWebDTO>(
            await _api.PostAsJsonAsync($"api/tramites-cobro/{id}/correo", new { Destino = destino }, LecturaEnvelope.Json)),
            "enviar la boleta por correo");
}
