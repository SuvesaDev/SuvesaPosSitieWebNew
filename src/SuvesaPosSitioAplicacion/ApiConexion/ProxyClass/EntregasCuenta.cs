using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IEntregasCuenta" />
public sealed class EntregasCuenta : ProxyBase, IEntregasCuenta
{
    private readonly ICobrosApiCliente _cobros;
    private readonly IFormasPagosApiCliente _formasPago;
    private readonly IClienteApiCliente _clientes;

    public EntregasCuenta(
        ICobrosApiCliente cobros,
        IFormasPagosApiCliente formasPago,
        IClienteApiCliente clientes,
        IContextoSesion sesion,
        ILogger<EntregasCuenta> log)
        : base(sesion, log)
    {
        _cobros = cobros;
        _formasPago = formasPago;
        _clientes = clientes;
    }

    public Task<ResponseGeneric<ICollection<FormasPagoDTO>>> FormasPago()
        => Ejecutar(async () =>
        {
            var r = await _formasPago.ObtenerFormasDePagoSinClienteAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las formas de pago");

    public Task<ResponseGeneric<ClienteBuscarNombreCedulaDTO>> BuscarClientePorCedula(long cedula)
        => Ejecutar(async () =>
        {
            var r = await _clientes.ObtenerClienteCedulaAsync(cedula);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar el cliente");

    public Task<ResponseGeneric<ICollection<EntregaCuentaDTO>>> Buscar(BuscarEntregaCuentaDTO filtro)
        => Ejecutar(async () =>
        {
            var r = await _cobros.BuscarEntregasAcuentaAsync(filtro);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar entregas a cuenta");

    public Task<ResponseGeneric<EntregaCuentaDTO>> Obtener(long id)
        => Ejecutar(async () =>
        {
            var r = await _cobros.ObtenerEntregasAcuentaAsync(id);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la entrega a cuenta");

    public Task<ResponseGeneric<EntregaCuentaDTO>> Crear(EntregaCuentaDTO entrega)
        => Ejecutar(async () =>
        {
            var r = await _cobros.InsertarEntregaAcuentaAsync(entrega);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "registrar la entrega a cuenta");
}
