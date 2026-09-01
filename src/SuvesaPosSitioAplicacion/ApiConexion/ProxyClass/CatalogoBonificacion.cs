using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Bonificacion;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICatalogoBonificacion" />
public sealed class CatalogoBonificacion : ProxyBase, ICatalogoBonificacion
{
    private readonly IConfiguracionBonificacionApiCliente _api;
    private readonly IBonificacionApiCliente _crud;

    public CatalogoBonificacion(
        IConfiguracionBonificacionApiCliente api,
        IBonificacionApiCliente crud,
        IContextoSesion sesion,
        ILogger<CatalogoBonificacion> log)
        : base(sesion, log)
    {
        _api = api;
        _crud = crud;
    }

    public Task<ResponseGeneric<ICollection<ConfiguracionBonificacion>>> Disponibles()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerConfiguracionesDisponiblesAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los tipos de bonificación");

    public Task<ResponseGeneric<ICollection<ConfiguracionBonificacionDTO>>> Todas()
        => Ejecutar(async () =>
        {
            var r = await _crud.TodasAsync();
            ICollection<ConfiguracionBonificacionDTO>? datos = r.Responses;
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, datos);
        }, "consultar los tipos de bonificación");

    public Task<ResponseGeneric<ConfiguracionBonificacionDTO>> Crear(ConfiguracionBonificacionDTO configuracion)
        => Ejecutar(async () =>
        {
            var r = await _crud.CrearAsync(configuracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear el tipo de bonificación");

    public Task<ResponseGeneric<ConfiguracionBonificacionDTO>> Editar(ConfiguracionBonificacionDTO configuracion)
        => Ejecutar(async () =>
        {
            var r = await _crud.EditarAsync(configuracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar el tipo de bonificación");

    public Task<ResponseGeneric<bool>> Habilitar(int idConfiguracion)
        => Ejecutar(async () =>
        {
            var r = await _crud.HabilitarAsync(idConfiguracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "habilitar el tipo de bonificación");

    public Task<ResponseGeneric<bool>> Deshabilitar(int idConfiguracion)
        => Ejecutar(async () =>
        {
            var r = await _crud.DeshabilitarAsync(idConfiguracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "deshabilitar el tipo de bonificación");

    public Task<ResponseGeneric<bool>> Eliminar(int idConfiguracion)
        => Ejecutar(async () =>
        {
            var r = await _crud.EliminarAsync(idConfiguracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "eliminar el tipo de bonificación");
}
