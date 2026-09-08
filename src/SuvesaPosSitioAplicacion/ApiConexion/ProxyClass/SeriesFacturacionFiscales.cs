using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;
namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;
public sealed class SeriesFacturacionFiscales : ProxyBase, ISeriesFacturacionFiscales
{
    private readonly HttpClient _api; private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public SeriesFacturacionFiscales(IHttpClientFactory factory, IContextoSesion sesion, ILogger<SeriesFacturacionFiscales> logger) : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");
    public Task<ResponseGeneric<ICollection<SerieFacturacionFiscalDTO>>> Obtener() => Ejecutar(async () => await Leer<ICollection<SerieFacturacionFiscalDTO>>(await _api.GetAsync("SeriesFacturacion/Obtener")), "consultar las series");
    public Task<ResponseGeneric<SeriesFacturacionCatalogosFiscalDTO>> Catalogos() => Ejecutar(async () => await Leer<SeriesFacturacionCatalogosFiscalDTO>(await _api.GetAsync("SeriesFacturacion/Catalogos")), "consultar los catálogos de series");
    public Task<ResponseGeneric<SerieFacturacionFiscalDTO>> Crear(SerieFacturacionFiscalDTO serie) => Enviar("SeriesFacturacion/Crear", serie, "crear la serie");
    public Task<ResponseGeneric<SerieFacturacionFiscalDTO>> Actualizar(SerieFacturacionFiscalDTO serie) => Enviar("SeriesFacturacion/Actualizar", serie, "actualizar la serie");
    private Task<ResponseGeneric<SerieFacturacionFiscalDTO>> Enviar(string ruta, SerieFacturacionFiscalDTO serie, string accion) => Ejecutar(async () => await Leer<SerieFacturacionFiscalDTO>(await _api.PostAsJsonAsync(ruta, serie, Json)), accion);
    private static async Task<ResponseGeneric<T>> Leer<T>(HttpResponseMessage respuesta) { var cuerpo = await respuesta.Content.ReadAsStringAsync(); if (!respuesta.IsSuccessStatusCode) return new($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}"); var envelope = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json) ?? throw new InvalidOperationException("Respuesta vacía."); return envelope.Status == 0 ? new(envelope.Responses) : new(envelope.CurrentException ?? "Error sin detalle.", envelope.ValidationErrors ?? Array.Empty<string>()); }
    private sealed class Envelope<T> { public int Status { get; init; } public string? CurrentException { get; init; } public IReadOnlyList<string>? ValidationErrors { get; init; } public T? Responses { get; init; } }
}
