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

    public DevolucionesCompra(IDevolucionCompraApiCliente api, IContextoSesion sesion, ILogger<DevolucionesCompra> log)
        : base(sesion, log)
    {
        _api = api;
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
}
