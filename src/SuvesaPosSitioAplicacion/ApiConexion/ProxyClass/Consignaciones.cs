using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IConsignaciones" />
///
/// NOTA: se usa venta/ObtenerConsignacionEstado y no
/// Consignacion/ObtenerConsignacionEncabezadoEstado. El segundo, que en principio
/// era el mas directo para esta pantalla, responde 500 contra devapi.pos2650.com
/// con cualquier valor del parametro e incluso sin el. Verificado con curl directo,
/// sin este codigo de por medio. Conviene reportarlo al equipo del API.
public sealed class Consignaciones : ProxyBase, IConsignaciones
{
    private readonly IConsignacionApiCliente _consignacion;
    private readonly IVentaApiCliente _venta;

    public Consignaciones(
        IConsignacionApiCliente consignacion,
        IVentaApiCliente venta,
        IContextoSesion sesion,
        ILogger<Consignaciones> log)
        : base(sesion, log)
    {
        _consignacion = consignacion;
        _venta = venta;
    }

    public Task<ResponseGeneric<ICollection<FacturaDTO>>> PorEstado(bool valor)
        => Ejecutar(async () =>
        {
            var r = await _consignacion.ObtenerConsignacionEstadoAsync(valor);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las consignaciones");

    public Task<ResponseGeneric<ICollection<ResultadoBusquedaConsignacionDTO>>> Buscar(string texto)
        => Ejecutar(async () =>
        {
            var limpio = texto?.Trim() ?? string.Empty;
            var porNumero = limpio.Length > 0 && limpio.All(char.IsDigit);

            var r = await _venta.BuscarConsignacionAsync(new BuscarConsignacionDTO
            {
                Numero = porNumero ? long.Parse(limpio) : null,
                NombreCliente = porNumero ? null : limpio,
                CedulaCliente = null
            });

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar consignaciones");
}
