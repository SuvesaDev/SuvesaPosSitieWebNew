using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IPresentaciones" />
public sealed class Presentaciones : ProxyBase, IPresentaciones
{
    private readonly IPresentacionApiCliente _api;

    public Presentaciones(IPresentacionApiCliente api, IContextoSesion sesion, ILogger<Presentaciones> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<Presentacione>>> Obtener()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerPresentacionesAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las presentaciones");

    public Task<ResponseGeneric<Presentacione>> Guardar(PresentacionDTO presentacion)
        => Ejecutar(async () =>
        {
            var r = await _api.PostPresentacionesAsync(presentacion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "guardar la presentacion");
}
