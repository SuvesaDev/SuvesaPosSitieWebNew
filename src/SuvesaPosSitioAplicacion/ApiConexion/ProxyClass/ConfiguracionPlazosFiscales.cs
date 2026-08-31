using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class ConfiguracionPlazosFiscales : ProxyBase, IConfiguracionPlazosFiscales
{
    private readonly HttpClient _api;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public ConfiguracionPlazosFiscales(IHttpClientFactory factory, IContextoSesion sesion, ILogger<ConfiguracionPlazosFiscales> logger)
        : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ICollection<ConfiguracionPlazoFiscalDTO>>> Obtener() =>
        Ejecutar(async () => await Leer<ICollection<ConfiguracionPlazoFiscalDTO>>(await _api.GetAsync("ConfiguracionPlazo/getPlazos")), "consultar los plazos");
    public Task<ResponseGeneric<ConfiguracionPlazoFiscalDTO>> Crear(ConfiguracionPlazoFiscalDTO plazo) => Enviar(HttpMethod.Post, "ConfiguracionPlazo/CreatePlazo", plazo, "crear el plazo");
    public Task<ResponseGeneric<ConfiguracionPlazoFiscalDTO>> Actualizar(ConfiguracionPlazoFiscalDTO plazo) => Enviar(HttpMethod.Put, "ConfiguracionPlazo/EditPlazo", plazo, "actualizar el plazo");
    public Task<ResponseGeneric<ConfiguracionPlazoFiscalDTO>> Deshabilitar(ConfiguracionPlazoFiscalDTO plazo) => Enviar(HttpMethod.Delete, "ConfiguracionPlazo/DeletePlazo", plazo, "deshabilitar el plazo");

    private Task<ResponseGeneric<ConfiguracionPlazoFiscalDTO>> Enviar(HttpMethod metodo, string ruta, ConfiguracionPlazoFiscalDTO plazo, string accion) => Ejecutar(async () =>
    {
        using var solicitud = new HttpRequestMessage(metodo, ruta) { Content = JsonContent.Create(plazo, options: Json) };
        return await Leer<ConfiguracionPlazoFiscalDTO>(await _api.SendAsync(solicitud));
    }, accion);

    private static async Task<ResponseGeneric<T>> Leer<T>(HttpResponseMessage respuesta) { var cuerpo = await respuesta.Content.ReadAsStringAsync(); if (!respuesta.IsSuccessStatusCode) return new($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}"); var envelope = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json) ?? throw new InvalidOperationException("Respuesta vacía."); return envelope.Status == 0 ? new(envelope.Responses) : new(envelope.CurrentException ?? "Error sin detalle.", envelope.ValidationErrors ?? Array.Empty<string>()); }
    private sealed class Envelope<T> { public int Status { get; init; } public string? CurrentException { get; init; } public IReadOnlyList<string>? ValidationErrors { get; init; } public T? Responses { get; init; } }
}
