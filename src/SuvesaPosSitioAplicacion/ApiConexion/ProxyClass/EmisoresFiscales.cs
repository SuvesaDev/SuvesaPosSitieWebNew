using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class EmisoresFiscales : ProxyBase, IEmisoresFiscales
{
    private readonly HttpClient _api;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public EmisoresFiscales(IHttpClientFactory factory, IContextoSesion sesion, ILogger<EmisoresFiscales> logger) : base(sesion, logger) => _api = factory.CreateClient("SeePosApi");
    public Task<ResponseGeneric<ICollection<EmisorFiscalDTO>>> Obtener() => Ejecutar(async () => await Leer<ICollection<EmisorFiscalDTO>>(await _api.PostAsync("Centros/ObtenerEmpresas", null)), "consultar los emisores");
    public Task<ResponseGeneric<EmisorFiscalDTO>> Actualizar(EmisorFiscalDTO emisor) => Ejecutar(async () => await Leer<EmisorFiscalDTO>(await _api.PostAsJsonAsync("Emisor/ActualizarEmpresa", emisor, Json)), "actualizar el emisor");
    public Task<ResponseGeneric<bool>> ActualizarCredenciales(CredencialesHaciendaFiscalDTO credenciales) => Ejecutar(async () => await Leer<bool>(await _api.PostAsJsonAsync("Emisor/ActualizarCredencialesHacienda", credenciales, Json)), "actualizar las credenciales de Hacienda");
    public Task<ResponseGeneric<EmisorLogoResumenDTO>> LogoMetadata(int idEmisor) => Ejecutar(async () => await Leer<EmisorLogoResumenDTO>(await _api.GetAsync($"api/emisores/{idEmisor}/logo/metadata")), "consultar el logo del emisor");
    public Task<ResponseGeneric<EmisorLogoResumenDTO>> GuardarLogo(int idEmisor, EmisorLogoActualizarDTO logo) => Ejecutar(async () => await Leer<EmisorLogoResumenDTO>(await _api.PutAsJsonAsync($"api/emisores/{idEmisor}/logo", logo, Json)), "guardar el logo del emisor");
    public Task<ResponseGeneric<bool>> EliminarLogo(int idEmisor) => Ejecutar(async () => await Leer<bool>(await _api.DeleteAsync($"api/emisores/{idEmisor}/logo")), "eliminar el logo del emisor");
    public Task<ResponseGeneric<EmisorLogoArchivoDTO>> DescargarLogo(int idEmisor) => Ejecutar(async () =>
    {
        var respuesta = await _api.GetAsync($"api/emisores/{idEmisor}/logo");
        if (!respuesta.IsSuccessStatusCode)
            return new ResponseGeneric<EmisorLogoArchivoDTO>($"El API respondió {(int)respuesta.StatusCode}: {await respuesta.Content.ReadAsStringAsync()}");

        return new ResponseGeneric<EmisorLogoArchivoDTO>(new EmisorLogoArchivoDTO
        {
            Contenido = await respuesta.Content.ReadAsByteArrayAsync(),
            MimeType = respuesta.Content.Headers.ContentType?.MediaType ?? "application/octet-stream"
        });
    }, "descargar el logo del emisor");
    private static async Task<ResponseGeneric<T>> Leer<T>(HttpResponseMessage respuesta) { var cuerpo = await respuesta.Content.ReadAsStringAsync(); if (!respuesta.IsSuccessStatusCode) return new($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}"); var envelope = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json) ?? throw new InvalidOperationException("Respuesta vacía."); return envelope.Status == 0 ? new(envelope.Responses) : new(envelope.CurrentException ?? "Error sin detalle.", envelope.ValidationErrors ?? Array.Empty<string>()); }
    private sealed class Envelope<T> { public int Status { get; init; } public string? CurrentException { get; init; } public IReadOnlyList<string>? ValidationErrors { get; init; } public T? Responses { get; init; } }
}
