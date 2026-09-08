using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="INotaCreditoCxC" />
public sealed class NotaCreditoCxC : ProxyBase, INotaCreditoCxC
{
    private readonly HttpClient _api;

    public NotaCreditoCxC(IHttpClientFactory factory, IContextoSesion sesion, ILogger<NotaCreditoCxC> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<int>> SerieUnica(int idEmisor, int idSucursal)
        => Ejecutar(async () => await LecturaEnvelope.Leer<int>(
            await _api.GetAsync($"api/nota-credito-cxc/serie-unica?idEmisor={idEmisor}&idSucursal={idSucursal}")),
            "resolver la serie única de nota de crédito");
}
