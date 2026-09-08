using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IComandosFacturacion" />
public sealed class ComandosFacturacion : ProxyBase, IComandosFacturacion
{
    private readonly HttpClient _api;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public ComandosFacturacion(IHttpClientFactory factory, IContextoSesion sesion, ILogger<ComandosFacturacion> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ResultadoOperacionFacturacionDTO>> CrearPreventaContado(FacturaDTO venta, string? claveIdempotencia)
        => Comando("api/facturacion/preventas/contado", venta, claveIdempotencia, "crear la preventa de contado");

    public Task<ResponseGeneric<ResultadoOperacionFacturacionDTO>> ConfirmarVentaCredito(FacturaDTO venta, string? claveIdempotencia)
        => Comando("api/facturacion/creditos", venta, claveIdempotencia, "confirmar la venta a crédito");

    public Task<ResponseGeneric<ResultadoOperacionFacturacionDTO>> CobrarVentaTiquete(FacturaDTO venta, string? claveIdempotencia)
        => Comando("api/facturacion/tiquetes", venta, claveIdempotencia, "cobrar el tiquete");

    public Task<ResponseGeneric<FacturarPreventaContadoResultadoDTO>> FacturarPreventaContado(FacturarPreventaContadoComandoDTO comando)
        => Ejecutar(async () => await Leer<FacturarPreventaContadoResultadoDTO>(
            await _api.PostAsJsonAsync("api/venta-orquestada/facturar-preventa-contado", comando, Json)),
            "cobrar y facturar la preventa de contado");

    public Task<ResponseGeneric<EstadoCuentaClienteDTO>> EstadoCuenta(long idCliente, DateTime? corte = null)
        => Ejecutar(async () =>
        {
            var ruta = $"api/cobros/estado-cuenta/{idCliente}";
            if (corte is { } c) ruta += $"?corte={Uri.EscapeDataString(c.ToString("O"))}";
            return await Leer<EstadoCuentaClienteDTO>(await _api.GetAsync(ruta));
        }, "consultar el estado de cuenta");

    private Task<ResponseGeneric<ResultadoOperacionFacturacionDTO>> Comando(
        string ruta, FacturaDTO venta, string? clave, string accion)
        => Ejecutar(async () =>
        {
            var cuerpo = new ComandoFacturacionDTO { ClaveIdempotencia = clave, Venta = venta };
            return await Leer<ResultadoOperacionFacturacionDTO>(await _api.PostAsJsonAsync(ruta, cuerpo, Json));
        }, accion);

    private static async Task<ResponseGeneric<T>> Leer<T>(HttpResponseMessage respuesta)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode) return new($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}");
        var envelope = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json)
            ?? throw new InvalidOperationException("El API devolvió una respuesta vacía.");
        return envelope.Status == 0
            ? new(envelope.Responses)
            : new(envelope.CurrentException ?? "El API devolvió un error sin detalle.",
                  envelope.ValidationErrors ?? Array.Empty<string>());
    }

    private sealed class Envelope<T>
    {
        public int Status { get; init; }
        public string? CurrentException { get; init; }
        public IReadOnlyList<string>? ValidationErrors { get; init; }
        public T? Responses { get; init; }
    }
}
