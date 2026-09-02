using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Lotes;

namespace SuvesaPosSitioAplicacion.ApiConexion;

/// <summary>
/// Cliente HTTP a mano para los endpoints nuevos de lotes / movimientos / toma
/// física (MEJORA_LOTES_API.md). TEMPORAL: al regenerar contratos NSwag se borra.
/// Comparte URL base y <c>ApiAuthHeaderHandler</c> (registrado en Program.cs).
/// </summary>
public interface ILotesApiCliente
{
    Task<LoteEnvelope<MovimientoInventarioPagina>> MovimientosAsync(MovimientoInventarioFiltro filtro);
    Task<LoteEnvelope<ExistenciaConsolidada>> ExistenciaConsolidadaAsync(long idArticulo);
    Task<LoteEnvelope<MovimientoInventarioResultado>> ActualizarExistenciaAsync(ActualizarExistencia req);

    Task<LoteEnvelope<List<TomaFisicaArticulo>>> TomaArticulosAsync(TomaFisicaFiltro filtro);
    Task<LoteEnvelope<TomaFisicaReporte>> TomaGuardarAsync(TomaFisicaGuardar req);
    Task<LoteEnvelope<TomaFisicaReporte>> TomaReporteAsync(long id);
}

/// <summary>Espejo de <c>ResponseGeneric&lt;T&gt;</c> del API.</summary>
public sealed class LoteEnvelope<T>
{
    [JsonPropertyName("status")] public ResponseStatus Status { get; set; }
    [JsonPropertyName("currentException")] public string? CurrentException { get; set; }
    [JsonPropertyName("validationErrors")] public List<string>? ValidationErrors { get; set; }
    [JsonPropertyName("responses")] public T? Responses { get; set; }
}

public sealed class LotesApiCliente : ILotesApiCliente
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;

    public LotesApiCliente(HttpClient http) => _http = http;

    private async Task<LoteEnvelope<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo = null)
    {
        using var req = new HttpRequestMessage(metodo, ruta);
        if (cuerpo is not null) req.Content = JsonContent.Create(cuerpo, options: Json);

        using var resp = await _http.SendAsync(req);
        var texto = await resp.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(texto))
            return new LoteEnvelope<T> { Status = ResponseStatus._1, CurrentException = $"El API respondió {(int)resp.StatusCode} sin cuerpo." };

        try
        {
            return JsonSerializer.Deserialize<LoteEnvelope<T>>(texto, Json)
                   ?? new LoteEnvelope<T> { Status = ResponseStatus._1, CurrentException = "Respuesta vacía." };
        }
        catch (JsonException)
        {
            return new LoteEnvelope<T> { Status = ResponseStatus._1, CurrentException = $"Respuesta no reconocida del API ({(int)resp.StatusCode})." };
        }
    }

    public Task<LoteEnvelope<MovimientoInventarioPagina>> MovimientosAsync(MovimientoInventarioFiltro filtro)
        => EnviarAsync<MovimientoInventarioPagina>(HttpMethod.Post, "InventarioMovimientos/Consultar", filtro);

    public Task<LoteEnvelope<ExistenciaConsolidada>> ExistenciaConsolidadaAsync(long idArticulo)
        => EnviarAsync<ExistenciaConsolidada>(HttpMethod.Get, $"InventarioMovimientos/ExistenciaConsolidada?idArticulo={idArticulo}");

    public Task<LoteEnvelope<MovimientoInventarioResultado>> ActualizarExistenciaAsync(ActualizarExistencia req)
        => EnviarAsync<MovimientoInventarioResultado>(HttpMethod.Put, "InventarioMovimientos/ActualizarExistencia", req);

    public Task<LoteEnvelope<List<TomaFisicaArticulo>>> TomaArticulosAsync(TomaFisicaFiltro filtro)
        => EnviarAsync<List<TomaFisicaArticulo>>(HttpMethod.Post, "TomaFisica/Articulos", filtro);

    public Task<LoteEnvelope<TomaFisicaReporte>> TomaGuardarAsync(TomaFisicaGuardar req)
        => EnviarAsync<TomaFisicaReporte>(HttpMethod.Post, "TomaFisica/Guardar", req);

    public Task<LoteEnvelope<TomaFisicaReporte>> TomaReporteAsync(long id)
        => EnviarAsync<TomaFisicaReporte>(HttpMethod.Get, $"TomaFisica/Reporte?id={id}");
}
