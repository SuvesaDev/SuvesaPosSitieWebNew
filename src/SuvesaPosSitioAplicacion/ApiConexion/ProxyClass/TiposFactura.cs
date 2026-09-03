using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="ITiposFactura" />
public sealed class TiposFacturaFiscal : ProxyBase, ITiposFactura
{
    private readonly HttpClient _api;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public TiposFacturaFiscal(IHttpClientFactory clientes, IContextoSesion sesion, ILogger<TiposFacturaFiscal> log)
        : base(sesion, log) => _api = clientes.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ICollection<TipoFacturaFiscalDTO>>> Obtener()
        => Ejecutar(async () =>
        {
            var r = await LeerAsync<ICollection<TipoFacturaFiscalDTO>>(await _api.GetAsync("TipoFactura/ObtenerTipoFacturas"));
            return r;
        }, "consultar los tipos de factura");

    public Task<ResponseGeneric<ICollection<TipoFacturaFiscalDTO>>> PorContexto(string contexto)
        => Ejecutar(async () => await LeerAsync<ICollection<TipoFacturaFiscalDTO>>(
            await _api.GetAsync($"TipoFactura/PorContexto?contexto={Uri.EscapeDataString(contexto)}")), "consultar los tipos de documento");

    public Task<ResponseGeneric<ICollection<CodigoFEDisponibleFiscalDTO>>> CodigosFEDisponibles()
        => Ejecutar(async () => await LeerAsync<ICollection<CodigoFEDisponibleFiscalDTO>>(
            await _api.GetAsync("TipoFactura/CodigosFEDisponibles")), "consultar los códigos FE disponibles");

    public Task<ResponseGeneric<TipoFacturaFiscalDTO>> Crear(TipoFacturaFiscalDTO tipo)
        => Ejecutar(async () =>
        {
            var r = await _api.PostAsJsonAsync("TipoFactura/CrearTipoFacturas", tipo, Json);
            return await LeerAsync<TipoFacturaFiscalDTO>(r);
        }, "crear el tipo de factura");

    public Task<ResponseGeneric<TipoFacturaFiscalDTO>> Actualizar(TipoFacturaFiscalDTO tipo)
        => Ejecutar(async () =>
        {
            var r = await _api.PostAsJsonAsync("TipoFactura/ActualizarTipoFacturas", tipo, Json);
            return await LeerAsync<TipoFacturaFiscalDTO>(r);
        }, "actualizar el tipo de factura");

    private static async Task<ResponseGeneric<T>> LeerAsync<T>(HttpResponseMessage respuesta)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode) return new ResponseGeneric<T>($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}");
        var envelope = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json)
            ?? throw new InvalidOperationException("El API devolvió una respuesta vacía.");
        return envelope.Status == 0
            ? new ResponseGeneric<T>(envelope.Responses)
            : new ResponseGeneric<T>(envelope.CurrentException ?? "El API devolvió un error sin detalle.", envelope.ValidationErrors ?? Array.Empty<string>());
    }

    private sealed class Envelope<T>
    {
        public int Status { get; init; }
        public string? CurrentException { get; init; }
        public IReadOnlyList<string>? ValidationErrors { get; init; }
        public T? Responses { get; init; }
    }
}
