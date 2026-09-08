using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICuentasPorCobrar" />
public sealed class CuentasPorCobrar : ProxyBase, ICuentasPorCobrar
{
    private readonly IAbonoCobrarApiCliente _api;

    public CuentasPorCobrar(
        IAbonoCobrarApiCliente api,
        IContextoSesion sesion,
        ILogger<CuentasPorCobrar> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<BuscarClientesPendientesDTO>>> ObtenerPendientes()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerFacturasPendientesCobrosAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las cuentas por cobrar");
}
