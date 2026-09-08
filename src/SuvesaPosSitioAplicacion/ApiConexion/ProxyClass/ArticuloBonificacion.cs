using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IArticuloBonificacion" />
public sealed class ArticuloBonificacion : ProxyBase, IArticuloBonificacion
{
    private readonly IArticuloBonificacionApiCliente _api;

    public ArticuloBonificacion(IArticuloBonificacionApiCliente api, IContextoSesion sesion, ILogger<ArticuloBonificacion> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<ArticuloBonificacionConfiguracion>>> ObtenerPorArticulo(long idInventario)
        => Ejecutar(async () =>
        {
            var r = await _api.GetConfiguracionAsync(idInventario);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la bonificación del artículo");

    public Task<ResponseGeneric<ArticuloBonificacionConfiguracion>> Crear(ArticuloBonificacionConfiguracion configuracion)
        => Ejecutar(async () =>
        {
            var r = await _api.CreateConfiguracionAsync(configuracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "guardar la bonificación del artículo");

    public Task<ResponseGeneric<ArticuloBonificacionConfiguracion>> Editar(ArticuloBonificacionConfiguracion configuracion)
        => Ejecutar(async () =>
        {
            var r = await _api.UpdateConfiguracionAsync(configuracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar la bonificación del artículo");

    public Task<ResponseGeneric<bool>> Eliminar(int idConfiguracion)
        => Ejecutar(async () =>
        {
            var r = await _api.DeleteConfiguracionAsync(idConfiguracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "eliminar la bonificación del artículo");
}
