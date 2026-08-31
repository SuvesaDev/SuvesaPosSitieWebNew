using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class DenominacionesMonedaFiscales : ProxyBase, IDenominacionesMonedaFiscales
{
    private readonly HttpClient _api;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public DenominacionesMonedaFiscales(IHttpClientFactory factory, IContextoSesion sesion, ILogger<DenominacionesMonedaFiscales> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ICollection<DenominacionMonedaFiscalDTO>>> Obtener() =>
        Ejecutar(async () => await Leer<ICollection<DenominacionMonedaFiscalDTO>>(await _api.GetAsync("DenominacionMoneda/Obtener")), "consultar las denominaciones");
    public Task<ResponseGeneric<DenominacionMonedaFiscalDTO>> Crear(DenominacionMonedaFiscalDTO denominacion) => Enviar("DenominacionMoneda/Crear", denominacion, "crear la denominación");
    public Task<ResponseGeneric<DenominacionMonedaFiscalDTO>> Actualizar(DenominacionMonedaFiscalDTO denominacion) => Enviar("DenominacionMoneda/Actualizar", denominacion, "actualizar la denominación");
    public Task<ResponseGeneric<DenominacionMonedaFiscalDTO>> Deshabilitar(long id) => Ejecutar(async () => await Leer<DenominacionMonedaFiscalDTO>(await _api.PostAsync($"DenominacionMoneda/Deshabilitar?idDenominacion={id}", null)), "deshabilitar la denominación");
    private Task<ResponseGeneric<DenominacionMonedaFiscalDTO>> Enviar(string ruta, DenominacionMonedaFiscalDTO denominacion, string accion) => Ejecutar(async () => await Leer<DenominacionMonedaFiscalDTO>(await _api.PostAsJsonAsync(ruta, denominacion, Json)), accion);
    private static async Task<ResponseGeneric<T>> Leer<T>(HttpResponseMessage respuesta) { var cuerpo = await respuesta.Content.ReadAsStringAsync(); if (!respuesta.IsSuccessStatusCode) return new($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}"); var envelope = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json) ?? throw new InvalidOperationException("Respuesta vacía."); return envelope.Status == 0 ? new(envelope.Responses) : new(envelope.CurrentException ?? "Error sin detalle.", envelope.ValidationErrors ?? Array.Empty<string>()); }
    private sealed class Envelope<T> { public int Status { get; init; } public string? CurrentException { get; init; } public IReadOnlyList<string>? ValidationErrors { get; init; } public T? Responses { get; init; } }
}
