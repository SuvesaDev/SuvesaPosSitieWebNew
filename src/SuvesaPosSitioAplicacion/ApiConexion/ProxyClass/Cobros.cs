using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICobros" />
public sealed class Cobros : ProxyBase, ICobros
{
    private readonly ICobrosApiCliente _cobros;
    private readonly IFormasPagosApiCliente _formasPago;
    private readonly IVentaApiCliente _venta;
    private readonly IClienteApiCliente _clientes;

    public Cobros(
        ICobrosApiCliente cobros,
        IFormasPagosApiCliente formasPago,
        IVentaApiCliente venta,
        IClienteApiCliente clientes,
        IContextoSesion sesion,
        ILogger<Cobros> log)
        : base(sesion, log)
    {
        _cobros = cobros;
        _formasPago = formasPago;
        _venta = venta;
        _clientes = clientes;
    }

    public Task<ResponseGeneric<ICollection<FormasPagoDTO>>> FormasPago(long codCliente)
        => Ejecutar(async () =>
        {
            var r = await _formasPago.ObtenerFormasDePagoAsync(codCliente);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las formas de pago");

    public Task<ResponseGeneric<PreventaDTO>> BuscarPorFicha(int ficha, DateTime fecha)
        => Ejecutar(async () =>
        {
            var r = await _venta.ObtenerPreventaPorFichaAsync(ficha, fecha);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar la preventa por ficha");

    public Task<ResponseGeneric<long>> CodigoClientePorCedula(string cedula)
        => Ejecutar(async () =>
        {
            var r = await _clientes.ObtenerCodigoClienteAsync(cedula);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar el código del cliente");

    public Task<ResponseGeneric<PreventaDTO>> BuscarPorCliente(long codCliente)
        => Ejecutar(async () =>
        {
            var r = await _venta.ObtenerPreventaPorCodigoClienteAsync(codCliente);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar la preventa del cliente");

    public Task<ResponseGeneric<ICollection<CobroDocumentosDTO>>> Cobrar(ICollection<CobroDocumentosDTO> cobros)
        => Ejecutar(async () =>
        {
            var r = await _cobros.InsertarCobroAsync(cobros);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "registrar el cobro");

    public Task<ResponseGeneric<FacturaDTO>> FacturarPreventa(long idPreventa)
        => Ejecutar(async () =>
        {
            var r = await _venta.PreventaFacturadaAsync(idPreventa);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "facturar la preventa");
}
