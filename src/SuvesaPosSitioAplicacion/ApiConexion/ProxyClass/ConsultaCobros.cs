using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IConsultaCobros" />
public sealed class ConsultaCobros : ProxyBase, IConsultaCobros
{
    private readonly HttpClient _api;

    public ConsultaCobros(IHttpClientFactory factory, IContextoSesion sesion, ILogger<ConsultaCobros> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IReadOnlyList<ReciboCobroResumenWebDTO>>> Recibos(
        DateTime? desde = null, DateTime? hasta = null, long? idCliente = null,
        int? idSucursal = null, long? numApertura = null, int? estado = null,
        long? numeroRecibo = null, int limite = 100)
        => Ejecutar(async () =>
        {
            var q = new List<string> { $"limite={limite}" };
            if (desde is { } d) q.Add($"desde={d:yyyy-MM-dd}");
            if (hasta is { } h) q.Add($"hasta={h:yyyy-MM-dd}");
            if (idCliente is { } c) q.Add($"idCliente={c}");
            if (idSucursal is { } s) q.Add($"idSucursal={s}");
            if (numApertura is { } a) q.Add($"numApertura={a}");
            if (estado is { } e) q.Add($"estado={e}");
            if (numeroRecibo is { } n) q.Add($"numeroRecibo={n}");
            return await LecturaEnvelope.Leer<IReadOnlyList<ReciboCobroResumenWebDTO>>(
                await _api.GetAsync("api/cobros/recibos?" + string.Join("&", q)));
        }, "consultar los recibos emitidos");

    public Task<ResponseGeneric<IReadOnlyList<OperacionFallidaWebDTO>>> OperacionesFallidas(int limite = 100)
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<OperacionFallidaWebDTO>>(
            await _api.GetAsync($"api/cobros/operaciones-fallidas?limite={limite}")),
            "consultar las operaciones fallidas");
}
