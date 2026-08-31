using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IClienteBonificacion" />
public sealed class ClienteBonificacion : ProxyBase, IClienteBonificacion
{
    private readonly IClienteBonificacionApiCliente _api;

    public ClienteBonificacion(IClienteBonificacionApiCliente api, IContextoSesion sesion, ILogger<ClienteBonificacion> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<ClienteBonificacionConfiguracionDTO>>> ObtenerPorCliente(string? cedula, long identificacion)
        => Ejecutar(async () =>
        {
            var r = await _api.GetConfiguracionClienteAsync(new BusquedaConfiguracionBonificacionClienteDTO
            {
                Cedula = cedula,
                Identificacion = identificacion
            });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la bonificación del cliente");

    public Task<ResponseGeneric<ClienteBonificacionConfiguracionDTO>> Crear(ClienteBonificacionConfiguracionDTO configuracion)
        => Ejecutar(async () =>
        {
            var r = await _api.CreateConfiguracion2Async(configuracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "guardar la bonificación del cliente");

    public Task<ResponseGeneric<ClienteBonificacionConfiguracionDTO>> Editar(ClienteBonificacionConfiguracionDTO configuracion)
        => Ejecutar(async () =>
        {
            var r = await _api.UpdateConfiguracion2Async(configuracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar la bonificación del cliente");

    public Task<ResponseGeneric<bool>> Eliminar(int idConfiguracion)
        => Ejecutar(async () =>
        {
            var r = await _api.DeleteConfiguracion2Async(idConfiguracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "eliminar la bonificación del cliente");
}
