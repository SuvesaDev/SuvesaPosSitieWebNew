using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ICotizaciones" />
public sealed class Cotizaciones : ProxyBase, ICotizaciones
{
    private readonly ICotizacionApiCliente _api;

    public Cotizaciones(
        ICotizacionApiCliente api,
        IContextoSesion sesion,
        ILogger<Cotizaciones> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<CotizacionesDTO>>> Obtener()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerCotizacionesAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las proformas");

    public Task<ResponseGeneric<CotizacionesDTO>> ObtenerPorId(
        long id, CotizacionesDTO? respaldoDelListado = null)
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerCotizacionPorIDAsync(id);

            // El endpoint heredado arma el DTO sin asignarle Cotizacion1 y luego
            // intenta cargar el detalle usando ese valor (0). El listado, en cambio,
            // si trae el identificador y las lineas. La pantalla ya tiene ese resumen,
            // asi que se usa como respaldo sin repetir la consulta completa.
            if (r.Responses is not null)
            {
                r.Responses.Cotizacion1 = id;

                if ((r.Responses.Detalle is null || r.Responses.Detalle.Count == 0)
                    && respaldoDelListado?.Cotizacion1 == id)
                {
                    r.Responses.Detalle = respaldoDelListado.Detalle?.ToList();
                }
            }

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la proforma");

    public Task<ResponseGeneric<CotizacionesDTO>> Crear(CotizacionesDTO cotizacion)
        => Ejecutar(async () =>
        {
            var r = await _api.CreateCotizacionAsync(cotizacion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la proforma");

    public Task<ResponseGeneric<CotizacionesDTO>> EditarEncabezado(CotizacionesDTO cotizacion)
        => Ejecutar(async () =>
        {
            var r = await _api.EditCotizacionEncabezadoAsync(cotizacion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar la proforma");

    public Task<ResponseGeneric<CotizacionesDTO>> Anular(CotizacionesDTO cotizacion)
        => Ejecutar(async () =>
        {
            var r = await _api.AnularCotizacionAsync(cotizacion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "anular la proforma");
}
