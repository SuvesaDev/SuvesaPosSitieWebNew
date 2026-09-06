using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IDevolucionesVenta" />
public sealed class DevolucionesVentaServicio : ProxyBase, IDevolucionesVenta
{
    private readonly IDevolucionVentasApiCliente _devoluciones;
    private readonly IVentaApiCliente _venta;
    private readonly IUsuarioApiCliente _usuarios;
    private readonly IMonedaApiCliente _monedas;
    private readonly HttpClient _api;

    public DevolucionesVentaServicio(
        IDevolucionVentasApiCliente devoluciones,
        IVentaApiCliente venta,
        IUsuarioApiCliente usuarios,
        IMonedaApiCliente monedas,
        IHttpClientFactory factory,
        IContextoSesion sesion,
        ILogger<DevolucionesVentaServicio> log)
        : base(sesion, log)
    {
        _devoluciones = devoluciones;
        _venta = venta;
        _usuarios = usuarios;
        _monedas = monedas;
        _api = factory.CreateClient("SeePosApi");
    }

    public Task<ResponseGeneric<FacturaDTO>> BuscarFacturaPorId(int idFactura)
        => Ejecutar(async () =>
        {
            var r = await _venta.ObtenerFacturaVentaDevolucionesAsync(idFactura);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar la factura");

    public Task<ResponseGeneric<FacturaDTO>> BuscarFacturaPorNumero(string numeroFactura)
        => Ejecutar(async () =>
        {
            var r = await _venta.ObtenerFacturaVentaDevolucionesPorNumeroFacturaAsync(numeroFactura);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar la factura");

    public Task<ResponseGeneric<ICollection<FacturaBuscarDevolucionesDTO>>> BuscarFacturasPorNumero(string numeroFactura)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ICollection<FacturaBuscarDevolucionesDTO>>(
            await _api.PostAsync(
                $"venta/ObtenerFacturasVentaDevolucionesPorNumeroFactura?numeroFactura={Uri.EscapeDataString(numeroFactura)}", null)),
            "buscar las facturas por número");

    public Task<ResponseGeneric<ICollection<FacturaBuscarDevolucionesDTO>>> BuscarFacturasPorFiltro(BuscarFacturaDevolucionesDTO filtro)
        => Ejecutar(async () =>
        {
            var r = await _venta.BusquedaFacturasDevolucionesAsync(filtro);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar facturas");

    public Task<ResponseGeneric<ICollection<DevolucionVentaDTO>>> Buscar(FiltroFacturaDevVenta filtro)
        => Ejecutar(async () =>
        {
            var r = await _devoluciones.ObtenerDevolucionVentaFiltrosAsync(filtro);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar devoluciones");

    public Task<ResponseGeneric<DevolucionVentaDTO>> ObtenerUna(long id)
        => Ejecutar(async () =>
        {
            var r = await _devoluciones.ObtenerDevolucionVentaPKAsync(id);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la devolución");

    public Task<ResponseGeneric<DevolucionVentaDTO>> Crear(DevolucionVentaDTO devolucion)
        => Ejecutar(async () =>
        {
            var r = await _devoluciones.CrearDevolucionVentaAsync(devolucion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "registrar la devolución");

    public Task<ResponseGeneric<ICollection<PersonalDTO>>> Personal()
        => Ejecutar(async () =>
        {
            var r = await _usuarios.ObtenerPersonalAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el personal");

    public Task<ResponseGeneric<ICollection<Moneda>>> Monedas()
        => Ejecutar(async () =>
        {
            var r = await _monedas.ObtenerMonedasInventarioAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las monedas");
}
