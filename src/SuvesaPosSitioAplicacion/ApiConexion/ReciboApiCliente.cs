using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.DTOs.Bodega;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion;

/// <summary>Cliente HTTP a mano para la cola de recibo de mercadería devuelta.</summary>
public interface IReciboApiCliente
{
    Task<LoteEnvelope<List<ReciboResumen>>> ColaAsync(ReciboFiltro filtro);
    Task<LoteEnvelope<ReciboOrden>> OrdenAsync(long id);
    Task<LoteEnvelope<ReciboOrden>> RecibirAsync(long idOrden, List<ReciboLineaRecibida> lineas, string? observaciones);
    Task<LoteEnvelope<ReciboOrden>> AnularAsync(long idOrden, string motivo);
}

public sealed class ReciboApiCliente : IReciboApiCliente
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly IContextoSesion _sesion;

    public ReciboApiCliente(HttpClient http, IContextoSesion sesion)
    {
        _http = http;
        _sesion = sesion;
    }

    private async Task<LoteEnvelope<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo = null)
    {
        await _sesion.CargarAsync();
        ContextoLlamada.Token = _sesion.Token;
        try
        {
            using var req = new HttpRequestMessage(metodo, ruta);
            if (cuerpo is not null) req.Content = JsonContent.Create(cuerpo, options: Json);
            using var resp = await _http.SendAsync(req);
            var texto = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(texto))
                return new() { Status = ResponseStatus._1, CurrentException = $"El API respondió {(int)resp.StatusCode} sin cuerpo." };
            try
            {
                var envelope = JsonSerializer.Deserialize<LoteEnvelope<T>>(texto, Json)
                       ?? new() { Status = ResponseStatus._1, CurrentException = "Respuesta vacía." };
                if (!resp.IsSuccessStatusCode && envelope.Status == ResponseStatus._0)
                {
                    envelope.Status = ResponseStatus._1;
                    envelope.CurrentException ??= $"El API respondió {(int)resp.StatusCode}.";
                }
                return envelope;
            }
            catch (JsonException ex)
            {
                var denegado = (int)resp.StatusCode == 403
                    ? "No tiene permiso para esta operación de recibo de mercadería."
                    : $"Respuesta no reconocida del API ({(int)resp.StatusCode}): {ex.Message}";
                return new() { Status = ResponseStatus._1, CurrentException = denegado };
            }
        }
        catch (Exception ex)
        {
            return new() { Status = ResponseStatus._1, CurrentException = ex.Message };
        }
        finally
        {
            ContextoLlamada.Token = null;
        }
    }

    public Task<LoteEnvelope<List<ReciboResumen>>> ColaAsync(ReciboFiltro filtro)
        => EnviarAsync<List<ReciboResumen>>(HttpMethod.Post, "ReciboMercaderia/Cola", filtro);

    public Task<LoteEnvelope<ReciboOrden>> OrdenAsync(long id)
        => EnviarAsync<ReciboOrden>(HttpMethod.Get, $"ReciboMercaderia/Orden?id={id}");

    public Task<LoteEnvelope<ReciboOrden>> RecibirAsync(long idOrden, List<ReciboLineaRecibida> lineas, string? observaciones)
        => EnviarAsync<ReciboOrden>(HttpMethod.Post, "ReciboMercaderia/Recibir", new { idOrden, lineas, observaciones });

    public Task<LoteEnvelope<ReciboOrden>> AnularAsync(long idOrden, string motivo)
        => EnviarAsync<ReciboOrden>(HttpMethod.Post, "ReciboMercaderia/Anular", new { idOrden, motivo });
}
