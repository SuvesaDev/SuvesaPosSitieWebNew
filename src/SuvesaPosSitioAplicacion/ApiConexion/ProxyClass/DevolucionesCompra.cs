using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IDevolucionesCompra" />
public sealed class DevolucionesCompra : ProxyBase, IDevolucionesCompra
{
    private readonly IDevolucionCompraApiCliente _api;
    private readonly HttpClient _http;

    public DevolucionesCompra(IDevolucionCompraApiCliente api, IHttpClientFactory factory, IContextoSesion sesion, ILogger<DevolucionesCompra> log)
        : base(sesion, log)
    {
        _api = api;
        _http = factory.CreateClient("SeePosApi");
    }

    public Task<ResponseGeneric<ICollection<DevolucionCompraDTO>>> Buscar(FiltroFacturaDevCompras filtro)
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerDevolucionCompraFiltrosAsync(filtro);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar devoluciones de compra");

    public Task<ResponseGeneric<DevolucionCompraDTO>> ObtenerUna(long id)
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerDevolucionCompraPKAsync(id);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la devolución de compra");

    public Task<ResponseGeneric<DevolucionCompraDTO>> Crear(DevolucionCompraDTO devolucion)
        => Ejecutar(async () =>
        {
            var r = await _api.CrearDevolucionCompraAsync(devolucion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "registrar la devolución de compra");

    public Task<ResponseGeneric<DevolucionCompraDTO>> Anular(long id)
        => Ejecutar(async () => await LecturaEnvelope.Leer<DevolucionCompraDTO>(
            await _http.PostAsync($"DevolucionCompra/Anular?id={id}", null)),
            "anular la devolución de compra");
}
