using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Produccion;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion;

public interface IProduccionApiCliente
{
    Task<LoteEnvelope<List<Bodega>>> BodegasAsync(); Task<LoteEnvelope<List<ProductoTerminado>>> ProductosTerminadosAsync(string? texto);
    Task<LoteEnvelope<List<FormulaComponente>>> FormulaAsync(long idPrincipal); Task<LoteEnvelope<bool>> GuardarComponenteAsync(GuardarComponenteFormula req);
    Task<LoteEnvelope<CalculoProduccion>> CalcularAsync(CalculoProduccionRequest req); Task<LoteEnvelope<ProduccionReporte>> ConvertirAsync(CalculoProduccionRequest req);
    Task<LoteEnvelope<ProduccionReporte>> ReporteAsync(long id); Task<LoteEnvelope<List<ProduccionReporte>>> ReportesAsync(BitacoraFiltro req); Task<LoteEnvelope<ProduccionReporte>> AnularAsync(AnularProduccion req);
}
public sealed class ProduccionApiCliente : IProduccionApiCliente
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web); private readonly HttpClient _http; private readonly IContextoSesion _sesion;
    public ProduccionApiCliente(HttpClient http, IContextoSesion sesion) { _http = http; _sesion = sesion; }
    private async Task<LoteEnvelope<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo = null) { await _sesion.CargarAsync(); ContextoLlamada.Token = _sesion.Token; try { using var req = new HttpRequestMessage(metodo, ruta); if (cuerpo is not null) req.Content = JsonContent.Create(cuerpo, options: Json); using var resp = await _http.SendAsync(req); var texto = await resp.Content.ReadAsStringAsync(); return string.IsNullOrWhiteSpace(texto) ? new() { Status = ResponseStatus._1, CurrentException = $"El API respondió {(int)resp.StatusCode} sin cuerpo." } : JsonSerializer.Deserialize<LoteEnvelope<T>>(texto, Json) ?? new() { Status = ResponseStatus._1, CurrentException = "Respuesta vacía." }; } catch (JsonException) { return new() { Status = ResponseStatus._1, CurrentException = "Respuesta no reconocida del API." }; } finally { ContextoLlamada.Token = null; } }
    // El endpoint del API es [HttpPost] (bodegaController), sin cuerpo. Se usan las
    // bodegas CostaPets, igual que el resto del flujo CostaPets.
    public Task<LoteEnvelope<List<Bodega>>> BodegasAsync() => EnviarAsync<List<Bodega>>(HttpMethod.Post, "Bodega/ObtenerBodegasCostaPets");
    public Task<LoteEnvelope<List<ProductoTerminado>>> ProductosTerminadosAsync(string? texto) => EnviarAsync<List<ProductoTerminado>>(HttpMethod.Post, "Produccion/ProductosTerminados", new ProductosTerminadosFiltro { Texto = texto });
    public Task<LoteEnvelope<List<FormulaComponente>>> FormulaAsync(long id) => EnviarAsync<List<FormulaComponente>>(HttpMethod.Get, $"Produccion/Formula?idPrincipal={id}"); public Task<LoteEnvelope<bool>> GuardarComponenteAsync(GuardarComponenteFormula r) => EnviarAsync<bool>(HttpMethod.Put, "Produccion/GuardarComponente", r);
    public Task<LoteEnvelope<CalculoProduccion>> CalcularAsync(CalculoProduccionRequest r) => EnviarAsync<CalculoProduccion>(HttpMethod.Post, "Produccion/Calcular", r); public Task<LoteEnvelope<ProduccionReporte>> ConvertirAsync(CalculoProduccionRequest r) => EnviarAsync<ProduccionReporte>(HttpMethod.Post, "Produccion/Convertir", r);
    public Task<LoteEnvelope<ProduccionReporte>> ReporteAsync(long id) => EnviarAsync<ProduccionReporte>(HttpMethod.Get, $"Produccion/Reporte?id={id}"); public Task<LoteEnvelope<List<ProduccionReporte>>> ReportesAsync(BitacoraFiltro r) => EnviarAsync<List<ProduccionReporte>>(HttpMethod.Post, "Produccion/Reportes", r); public Task<LoteEnvelope<ProduccionReporte>> AnularAsync(AnularProduccion r) => EnviarAsync<ProduccionReporte>(HttpMethod.Post, "Produccion/Anular", r);
}
