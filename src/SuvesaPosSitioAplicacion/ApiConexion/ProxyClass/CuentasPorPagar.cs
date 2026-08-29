using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICuentasPorPagar" />
public sealed class CuentasPorPagar : ProxyBase, ICuentasPorPagar
{
    private readonly IAbonoPagarApiCliente _api;

    public CuentasPorPagar(
        IAbonoPagarApiCliente api,
        IContextoSesion sesion,
        ILogger<CuentasPorPagar> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<BuscarProveedorPendientesDTO>>> ObtenerDeudas()
        => Ejecutar(async () =>
        {
            var r = await _api.GetDatosProveedoresDeudasAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las cuentas por pagar");

    public Task<ResponseGeneric<AbonoCuentaPagarReciboDTO>> CrearAbono(AbonoCuentaPagarReciboDTO abono)
        => Ejecutar(async () =>
        {
            var r = await _api.CreateAbonoPagarAsync(abono);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "registrar el abono al proveedor");
}
