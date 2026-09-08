using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuvesaPosSitioAplicacion.DTOs.Bonificacion;
using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.ApiConexion;

/// <summary>
/// Cliente HTTP tipado para el CRUD del catalogo de bonificaciones
/// (<c>/ConfiguracionBonificacion/*</c>, API §3.1).
///
/// TEMPORAL — hecho a mano porque los contratos NSwag del sitio no se pueden
/// regenerar en local. Al correr <c>./tools/actualizar-contratos.sh</c> contra el
/// API nuevo desplegado, esto lo sustituye el cliente generado y este archivo se
/// borra. Comparte URL base y <c>ApiAuthHeaderHandler</c> con el resto de
/// clientes (registrado igual en Program.cs).
/// </summary>
public interface IBonificacionApiCliente
{
    Task<BonificacionEnvelope<List<ConfiguracionBonificacionDTO>>> TodasAsync();
    Task<BonificacionEnvelope<ConfiguracionBonificacionDTO>> CrearAsync(ConfiguracionBonificacionDTO dto);
    Task<BonificacionEnvelope<ConfiguracionBonificacionDTO>> EditarAsync(ConfiguracionBonificacionDTO dto);
    Task<BonificacionEnvelope<bool>> HabilitarAsync(int id);
    Task<BonificacionEnvelope<bool>> DeshabilitarAsync(int id);
    Task<BonificacionEnvelope<bool>> EliminarAsync(int id);
}

/// <summary>Espejo del <c>ResponseGeneric&lt;T&gt;</c> del API.</summary>
public sealed class BonificacionEnvelope<T>
{
    [JsonPropertyName("status")] public ResponseStatus Status { get; set; }
    [JsonPropertyName("currentException")] public string? CurrentException { get; set; }
    [JsonPropertyName("validationErrors")] public List<string>? ValidationErrors { get; set; }
    [JsonPropertyName("responses")] public T? Responses { get; set; }
}

public sealed class BonificacionApiCliente : IBonificacionApiCliente
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;

    public BonificacionApiCliente(HttpClient http) => _http = http;

    private async Task<BonificacionEnvelope<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo = null)
    {
        using var req = new HttpRequestMessage(metodo, ruta);
        if (cuerpo is not null)
            req.Content = JsonContent.Create(cuerpo, options: Json);

        using var resp = await _http.SendAsync(req);
        var texto = await resp.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(texto))
            return new BonificacionEnvelope<T> { Status = ResponseStatus._1, CurrentException = $"El API respondio {(int)resp.StatusCode} sin cuerpo." };

        try
        {
            return JsonSerializer.Deserialize<BonificacionEnvelope<T>>(texto, Json)
                   ?? new BonificacionEnvelope<T> { Status = ResponseStatus._1, CurrentException = "Respuesta vacia." };
        }
        catch (JsonException)
        {
            return new BonificacionEnvelope<T> { Status = ResponseStatus._1, CurrentException = $"Respuesta no reconocida del API ({(int)resp.StatusCode})." };
        }
    }

    public Task<BonificacionEnvelope<List<ConfiguracionBonificacionDTO>>> TodasAsync()
        => EnviarAsync<List<ConfiguracionBonificacionDTO>>(HttpMethod.Get, "ConfiguracionBonificacion/ObtenerTodasLasConfiguraciones");

    public Task<BonificacionEnvelope<ConfiguracionBonificacionDTO>> CrearAsync(ConfiguracionBonificacionDTO dto)
        => EnviarAsync<ConfiguracionBonificacionDTO>(HttpMethod.Post, "ConfiguracionBonificacion/CrearConfiguracion", dto);

    public Task<BonificacionEnvelope<ConfiguracionBonificacionDTO>> EditarAsync(ConfiguracionBonificacionDTO dto)
        => EnviarAsync<ConfiguracionBonificacionDTO>(HttpMethod.Put, "ConfiguracionBonificacion/EditarConfiguracion", dto);

    public Task<BonificacionEnvelope<bool>> HabilitarAsync(int id)
        => EnviarAsync<bool>(HttpMethod.Put, $"ConfiguracionBonificacion/HabilitarConfiguracion?id={id}");

    public Task<BonificacionEnvelope<bool>> DeshabilitarAsync(int id)
        => EnviarAsync<bool>(HttpMethod.Put, $"ConfiguracionBonificacion/DeshabilitarConfiguracion?id={id}");

    public Task<BonificacionEnvelope<bool>> EliminarAsync(int id)
        => EnviarAsync<bool>(HttpMethod.Delete, $"ConfiguracionBonificacion/EliminarConfiguracion?id={id}");
}
