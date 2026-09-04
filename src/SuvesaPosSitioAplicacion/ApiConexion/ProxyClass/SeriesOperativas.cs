using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ISeriesOperativas" />
public sealed class SeriesOperativas : ProxyBase, ISeriesOperativas
{
    private readonly HttpClient _api;

    public SeriesOperativas(IHttpClientFactory factory, IContextoSesion sesion, ILogger<SeriesOperativas> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IReadOnlyList<SerieOperativaWebDTO>>> Listar(int? tipo = null, int? idEmisor = null, int? idSucursal = null)
        => Ejecutar(async () =>
        {
            var q = new List<string>();
            if (tipo is not null) q.Add($"tipo={tipo}");
            if (idEmisor is not null) q.Add($"idEmisor={idEmisor}");
            if (idSucursal is not null) q.Add($"idSucursal={idSucursal}");
            var url = "api/series-operativas" + (q.Count > 0 ? "?" + string.Join("&", q) : "");
            return await LecturaEnvelope.Leer<IReadOnlyList<SerieOperativaWebDTO>>(await _api.GetAsync(url));
        }, "consultar las series operativas");

    public Task<ResponseGeneric<int>> Guardar(SerieOperativaWebDTO dto)
        => Ejecutar(async () => await LecturaEnvelope.Leer<int>(
            await _api.PostAsJsonAsync("api/series-operativas", dto, LecturaEnvelope.Json)),
            "guardar la serie operativa");

    public Task<ResponseGeneric<bool>> Activar(int id, bool activa)
        => Ejecutar(async () => await LecturaEnvelope.Leer<bool>(
            await _api.PostAsync($"api/series-operativas/{id}/activar?activa={activa.ToString().ToLowerInvariant()}", null)),
            "activar o desactivar la serie operativa");

    public Task<ResponseGeneric<IReadOnlyList<HallazgoConfiguracionWebDTO>>> Diagnostico()
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<HallazgoConfiguracionWebDTO>>(
            await _api.GetAsync("api/series-operativas/diagnostico")), "consultar el diagnóstico de configuración");
}
