using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ITipoCambio" />
public sealed class TipoCambio : ProxyBase, ITipoCambio
{
    private readonly HttpClient _api;

    public TipoCambio(IHttpClientFactory factory, IContextoSesion sesion, ILogger<TipoCambio> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<TipoCambioOficialDTO>> Oficial(DateTime? fecha = null) => Ejecutar(async () =>
    {
        var q = fecha is { } f ? $"?fecha={f:yyyy-MM-dd}" : string.Empty;
        return await LecturaEnvelope.Leer<TipoCambioOficialDTO>(await _api.GetAsync($"tipo-cambio/oficial{q}"));
    }, "consultar el tipo de cambio oficial");
}
