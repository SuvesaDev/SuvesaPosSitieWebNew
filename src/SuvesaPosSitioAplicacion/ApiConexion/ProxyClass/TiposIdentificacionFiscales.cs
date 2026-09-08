using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Fiscal;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

public sealed class TiposIdentificacionFiscales : ProxyBase, ITiposIdentificacionFiscales
{
    private readonly HttpClient _api;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public TiposIdentificacionFiscales(IHttpClientFactory clientes, IContextoSesion sesion, ILogger<TiposIdentificacionFiscales> log)
        : base(sesion, log) => _api = clientes.CreateClient("SeePosApi");

    public Task<ResponseGeneric<ICollection<TipoIdentificacionFiscalDTO>>> Obtener() => Ejecutar(async () =>
        await LeerAsync<ICollection<TipoIdentificacionFiscalDTO>>(await _api.PostAsync("Identificacion/Obtener", null)), "consultar los tipos de identificación");
    public Task<ResponseGeneric<TipoIdentificacionFiscalDTO>> Crear(TipoIdentificacionFiscalDTO tipo) => Enviar("Identificacion/Crear", tipo, "crear el tipo de identificación");
    public Task<ResponseGeneric<TipoIdentificacionFiscalDTO>> Actualizar(TipoIdentificacionFiscalDTO tipo) => Enviar("Identificacion/Actualizar", tipo, "actualizar el tipo de identificación");
    public Task<ResponseGeneric<TipoIdentificacionFiscalDTO>> Deshabilitar(TipoIdentificacionFiscalDTO tipo) => Enviar("Identificacion/Deshabilitar", tipo, "deshabilitar el tipo de identificación");

    private Task<ResponseGeneric<TipoIdentificacionFiscalDTO>> Enviar(string ruta, TipoIdentificacionFiscalDTO tipo, string accion) => Ejecutar(async () =>
        await LeerAsync<TipoIdentificacionFiscalDTO>(await _api.PostAsJsonAsync(ruta, tipo, Json)), accion);

    private static async Task<ResponseGeneric<T>> LeerAsync<T>(HttpResponseMessage respuesta)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode) return new ResponseGeneric<T>($"El API respondió {(int)respuesta.StatusCode}: {cuerpo}");
        var envelope = JsonSerializer.Deserialize<Envelope<T>>(cuerpo, Json) ?? throw new InvalidOperationException("El API devolvió una respuesta vacía.");
        return envelope.Status == 0 ? new ResponseGeneric<T>(envelope.Responses) : new ResponseGeneric<T>(envelope.CurrentException ?? "El API devolvió un error sin detalle.", envelope.ValidationErrors ?? Array.Empty<string>());
    }

    private sealed class Envelope<T> { public int Status { get; init; } public string? CurrentException { get; init; } public IReadOnlyList<string>? ValidationErrors { get; init; } public T? Responses { get; init; } }
}
