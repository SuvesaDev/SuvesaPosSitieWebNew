using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Cobros;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IFormasPagoConfig" />
public sealed class FormasPagoConfig : ProxyBase, IFormasPagoConfig
{
    private readonly HttpClient _api;

    public FormasPagoConfig(IHttpClientFactory factory, IContextoSesion sesion, ILogger<FormasPagoConfig> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<IReadOnlyList<FormaPagoConfigWebDTO>>> Listar()
        => Ejecutar(async () => await LecturaEnvelope.Leer<IReadOnlyList<FormaPagoConfigWebDTO>>(
            await _api.PostAsync("FormasPagos/ObtenerFormasDePagoSinCliente", null)),
            "consultar las formas de pago");

    public Task<ResponseGeneric<FormaPagoConfigWebDTO>> Guardar(FormaPagoConfigWebDTO forma)
        => Ejecutar(async () => await LecturaEnvelope.Leer<FormaPagoConfigWebDTO>(
            await _api.PostAsJsonAsync("FormasPagos/Update", forma, LecturaEnvelope.Json)),
            "guardar la forma de pago");
}
