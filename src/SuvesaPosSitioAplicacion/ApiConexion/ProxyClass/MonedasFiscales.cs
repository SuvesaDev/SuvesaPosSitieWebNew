using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class MonedasFiscales : ProxyBase, IMonedasFiscales
{
    private readonly HttpClient _api;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public MonedasFiscales(IHttpClientFactory factory, IContextoSesion sesion, ILogger<MonedasFiscales> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ICollection<MonedaFiscalDTO>>> Obtener() =>
        Ejecutar(async () => await Leer<ICollection<MonedaFiscalDTO>>(await _api.PostAsync("moneda/ObtenerMonedasInventario", null)), "consultar las monedas");

    public Task<ResponseGeneric<MonedaFiscalDTO>> Crear(MonedaFiscalDTO moneda) =>
        Enviar("moneda/Crear", moneda, "crear la moneda");

    public Task<ResponseGeneric<MonedaFiscalDTO>> Actualizar(MonedaFiscalDTO moneda) =>
        Enviar("moneda/Actualizar", moneda, "actualizar la moneda");

    public Task<ResponseGeneric<MonedaFiscalDTO>> Deshabilitar(int codigo, string? usuario) =>
        Ejecutar(async () => await Leer<MonedaFiscalDTO>(await _api.PostAsync($"moneda/Deshabilitar?codMoneda={codigo}&usuario={Uri.EscapeDataString(usuario ?? string.Empty)}", null)), "deshabilitar la moneda");

    private Task<ResponseGeneric<MonedaFiscalDTO>> Enviar(string ruta, MonedaFiscalDTO moneda, string accion) =>
        Ejecutar(async () => await Leer<MonedaFiscalDTO>(await _api.PostAsJsonAsync(ruta, moneda, Json)), accion);

    private static async Task<ResponseGeneric<T>> Leer<T>(HttpResponseMessage respuesta)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode) return new($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}");
        var envelope = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json) ?? throw new InvalidOperationException("Respuesta vacía.");
        return envelope.Status == 0 ? new(envelope.Responses) : new(envelope.CurrentException ?? "Error sin detalle.", envelope.ValidationErrors ?? Array.Empty<string>());
    }

    private sealed class Envelope<T> { public int Status { get; init; } public string? CurrentException { get; init; } public IReadOnlyList<string>? ValidationErrors { get; init; } public T? Responses { get; init; } }
}
