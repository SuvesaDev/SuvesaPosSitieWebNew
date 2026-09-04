using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Compras;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IOrdenesCompraFlujo" />
public sealed class OrdenesCompraFlujo : ProxyBase, IOrdenesCompraFlujo
{
    private readonly HttpClient _api;

    public OrdenesCompraFlujo(IHttpClientFactory factory, IContextoSesion sesion, ILogger<OrdenesCompraFlujo> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<long>> SiguienteConsecutivo(int idEmisor, int idSucursal)
        => Ejecutar(async () => await LecturaEnvelope.Leer<long>(
            await _api.GetAsync($"api/ordenes-compra/siguiente-consecutivo?idEmisor={idEmisor}&idSucursal={idSucursal}")),
            "consultar el consecutivo de la orden de compra");

    public Task<ResponseGeneric<OrdenCompraFlujoWebDTO>> Crear(CrearOrdenCompraWebDTO cmd)
        => Ejecutar(async () => await LecturaEnvelope.Leer<OrdenCompraFlujoWebDTO>(
            await _api.PostAsJsonAsync("api/ordenes-compra", cmd, LecturaEnvelope.Json)), "crear la orden de compra");

    public Task<ResponseGeneric<IReadOnlyList<OrdenCompraFlujoWebDTO>>> Listar(
        int? idProveedor = null, int? estado = null, bool incluirAnuladas = false,
        DateTime? desde = null, DateTime? hasta = null, long? consecutivo = null, int limite = 200)
        => Ejecutar(async () =>
        {
            var q = new List<string> { $"incluirAnuladas={incluirAnuladas.ToString().ToLowerInvariant()}", $"limite={limite}" };
            if (idProveedor is { } p) q.Add($"idProveedor={p}");
            if (estado is { } e) q.Add($"estado={e}");
            if (desde is { } d) q.Add($"desde={d:yyyy-MM-dd}");
            if (hasta is { } h) q.Add($"hasta={h:yyyy-MM-dd}");
            if (consecutivo is { } c) q.Add($"consecutivo={c}");
            return await LecturaEnvelope.Leer<IReadOnlyList<OrdenCompraFlujoWebDTO>>(
                await _api.GetAsync("api/ordenes-compra?" + string.Join("&", q)));
        }, "consultar las órdenes de compra");

    public Task<ResponseGeneric<OrdenCompraFlujoWebDTO>> Obtener(long orden)
        => Ejecutar(async () => await LecturaEnvelope.Leer<OrdenCompraFlujoWebDTO>(
            await _api.GetAsync($"api/ordenes-compra/{orden}")), "consultar la orden de compra");

    public Task<ResponseGeneric<OrdenCompraFlujoWebDTO>> Entregar(long orden, DateTime? fecha)
        => Ejecutar(async () => await LecturaEnvelope.Leer<OrdenCompraFlujoWebDTO>(
            await _api.PostAsync($"api/ordenes-compra/{orden}/entregar" + (fecha is { } f ? $"?fecha={f:yyyy-MM-dd}" : ""), null)),
            "marcar la orden como entregada");

    public Task<ResponseGeneric<OrdenCompraFlujoWebDTO>> Cancelar(long orden, string? motivo)
        => Ejecutar(async () => await LecturaEnvelope.Leer<OrdenCompraFlujoWebDTO>(
            await _api.PostAsJsonAsync($"api/ordenes-compra/{orden}/cancelar", new { Motivo = motivo }, LecturaEnvelope.Json)),
            "cancelar la orden de compra");

    public Task<ResponseGeneric<OrdenCompraFlujoWebDTO>> BajaProveedor(long orden, string? motivo)
        => Ejecutar(async () => await LecturaEnvelope.Leer<OrdenCompraFlujoWebDTO>(
            await _api.PostAsJsonAsync($"api/ordenes-compra/{orden}/baja-proveedor", new { Motivo = motivo }, LecturaEnvelope.Json)),
            "dar de baja la orden de compra");

    public Task<ResponseGeneric<OrdenCompraFlujoWebDTO>> VincularFactura(long orden, long idFacturaCompra)
        => Ejecutar(async () => await LecturaEnvelope.Leer<OrdenCompraFlujoWebDTO>(
            await _api.PostAsJsonAsync($"api/ordenes-compra/{orden}/vincular-factura", new { IdFacturaCompra = idFacturaCompra }, LecturaEnvelope.Json)),
            "vincular la factura de compra");

    public Task<ResponseGeneric<ResultadoEnvioOrdenCompraWebDTO>> EnviarCorreo(long orden, string? destino)
        => Ejecutar(async () => await LecturaEnvelope.Leer<ResultadoEnvioOrdenCompraWebDTO>(
            await _api.PostAsJsonAsync($"api/ordenes-compra/{orden}/correo", new { Destino = destino }, LecturaEnvelope.Json)),
            "enviar la orden de compra por correo");
}
