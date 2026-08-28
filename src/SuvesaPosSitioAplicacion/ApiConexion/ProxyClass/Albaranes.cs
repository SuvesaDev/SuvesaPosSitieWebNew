using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IAlbaranes" />
public sealed class Albaranes : ProxyBase, IAlbaranes
{
    private readonly IQvetApiCliente _api;

    public Albaranes(IQvetApiCliente api, IContextoSesion sesion, ILogger<Albaranes> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<VentaDTO>>> PendientesDeFacturar()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerAlbaranesPendientesFacturarAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los albaranes pendientes");

    public Task<ResponseGeneric<ICollection<VentaDTO>>> Todos()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerAlbaranesTodosAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los albaranes");

    public Task<ResponseGeneric<VentaDTO>> Uno(long id)
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerAlbaranAsync(id);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el albaran");

    public Task<ResponseGeneric<ICollection<EstadoAlbaranesDTO>>> Estados()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerEstadosAlbaranesAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los estados de albaranes");
}
