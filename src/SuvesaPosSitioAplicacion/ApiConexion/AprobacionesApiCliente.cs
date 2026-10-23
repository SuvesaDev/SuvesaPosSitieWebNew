using System.Net.Http.Json;
using System.Text.Json;
using SuvesaPosSitioAplicacion.DTOs.Contabilidad;
using SuvesaPosSitioAplicacion.DTOs.Generated;

namespace SuvesaPosSitioAplicacion.ApiConexion;

/// <summary>
/// Cliente HTTP tipado para <c>api/contabilidad/aprobaciones|tipos-cambio|revaluaciones</c>.
///
/// TEMPORAL — hecho a mano igual que <see cref="SeguridadApiCliente"/>: los contratos NSwag
/// no se regeneran en local. Al regenerarlos contra el API nuevo, esto se sustituye.
/// </summary>
public interface IAprobacionesApiCliente
{
    Task<SeguridadEnvelope<ConfiguracionAprobacionDTO>> ConfiguracionAsync(long idLibro);
    Task<SeguridadEnvelope<bool>> CambiarModoAsync(CambiarModoAprobacionDTO dto);
    Task<SeguridadEnvelope<bool>> AsignarRolAsync(AsignarRolContableDTO dto);
    Task<SeguridadEnvelope<bool>> QuitarRolAsync(long idRol);
    Task<SeguridadEnvelope<SolicitudAprobacionDTO>> SolicitarAsync(SolicitarAprobacionDTO dto);
    Task<SeguridadEnvelope<List<SolicitudAprobacionDTO>>> SolicitudesAsync(long idLibro, string? estado);
    Task<SeguridadEnvelope<SolicitudAprobacionDTO>> ResolverAsync(long idSolicitud, ResolverSolicitudDTO dto);
    Task<SeguridadEnvelope<bool>> CancelarAsync(long idSolicitud);
    Task<SeguridadEnvelope<RevisionSemanalDTO>> RevisionSemanalAsync(long idLibro, DateOnly dia);
    Task<SeguridadEnvelope<RevisionSemanalDTO>> FirmarRevisionSemanalAsync(FirmarRevisionSemanalDTO dto);
    Task<SeguridadEnvelope<List<TipoCambioContableDTO>>> TiposCambioAsync(string moneda, DateOnly desde, DateOnly hasta);
    Task<SeguridadEnvelope<TipoCambioContableDTO>> RegistrarTipoCambioAsync(RegistrarTipoCambioDTO dto);
    Task<SeguridadEnvelope<List<RevaluacionDTO>>> RevaluacionesAsync(long idLibro);
    Task<SeguridadEnvelope<RevaluacionDTO>> SimularRevaluacionAsync(EjecutarRevaluacionDTO dto);
    Task<SeguridadEnvelope<RevaluacionDTO>> EjecutarRevaluacionAsync(EjecutarRevaluacionDTO dto);
}

public sealed class AprobacionesApiCliente : IAprobacionesApiCliente
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private const string Base = "api/contabilidad/";

    private readonly HttpClient _http;

    public AprobacionesApiCliente(HttpClient http) => _http = http;

    private async Task<SeguridadEnvelope<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo = null)
    {
        using var req = new HttpRequestMessage(metodo, Base + ruta);
        if (cuerpo is not null)
            req.Content = JsonContent.Create(cuerpo, options: Json);

        using var resp = await _http.SendAsync(req);
        var texto = await resp.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(texto))
            return new SeguridadEnvelope<T> { Status = ResponseStatus._1, CurrentException = $"El API respondio {(int)resp.StatusCode} sin cuerpo." };
        try
        {
            return JsonSerializer.Deserialize<SeguridadEnvelope<T>>(texto, Json)
                   ?? new SeguridadEnvelope<T> { Status = ResponseStatus._1, CurrentException = "Respuesta vacia." };
        }
        catch (JsonException)
        {
            return new SeguridadEnvelope<T> { Status = ResponseStatus._1, CurrentException = $"Respuesta no reconocida del API ({(int)resp.StatusCode})." };
        }
    }

    private static string Fecha(DateOnly f) => f.ToString("yyyy-MM-dd");

    public Task<SeguridadEnvelope<ConfiguracionAprobacionDTO>> ConfiguracionAsync(long idLibro)
        => EnviarAsync<ConfiguracionAprobacionDTO>(HttpMethod.Get, $"aprobaciones/configuracion?idLibroContable={idLibro}");

    public Task<SeguridadEnvelope<bool>> CambiarModoAsync(CambiarModoAprobacionDTO dto)
        => EnviarAsync<bool>(HttpMethod.Put, "aprobaciones/modo", dto);

    public Task<SeguridadEnvelope<bool>> AsignarRolAsync(AsignarRolContableDTO dto)
        => EnviarAsync<bool>(HttpMethod.Post, "aprobaciones/roles", dto);

    public Task<SeguridadEnvelope<bool>> QuitarRolAsync(long idRol)
        => EnviarAsync<bool>(HttpMethod.Delete, $"aprobaciones/roles/{idRol}");

    public Task<SeguridadEnvelope<SolicitudAprobacionDTO>> SolicitarAsync(SolicitarAprobacionDTO dto)
        => EnviarAsync<SolicitudAprobacionDTO>(HttpMethod.Post, "aprobaciones/solicitudes", dto);

    public Task<SeguridadEnvelope<List<SolicitudAprobacionDTO>>> SolicitudesAsync(long idLibro, string? estado)
        => EnviarAsync<List<SolicitudAprobacionDTO>>(HttpMethod.Get,
            $"aprobaciones/solicitudes?idLibroContable={idLibro}" + (string.IsNullOrWhiteSpace(estado) ? "" : $"&estado={Uri.EscapeDataString(estado)}"));

    public Task<SeguridadEnvelope<SolicitudAprobacionDTO>> ResolverAsync(long idSolicitud, ResolverSolicitudDTO dto)
        => EnviarAsync<SolicitudAprobacionDTO>(HttpMethod.Post, $"aprobaciones/solicitudes/{idSolicitud}/resolver", dto);

    public Task<SeguridadEnvelope<bool>> CancelarAsync(long idSolicitud)
        => EnviarAsync<bool>(HttpMethod.Post, $"aprobaciones/solicitudes/{idSolicitud}/cancelar");

    public Task<SeguridadEnvelope<RevisionSemanalDTO>> RevisionSemanalAsync(long idLibro, DateOnly dia)
        => EnviarAsync<RevisionSemanalDTO>(HttpMethod.Get, $"aprobaciones/revision-semanal?idLibroContable={idLibro}&dia={Fecha(dia)}");

    public Task<SeguridadEnvelope<RevisionSemanalDTO>> FirmarRevisionSemanalAsync(FirmarRevisionSemanalDTO dto)
        => EnviarAsync<RevisionSemanalDTO>(HttpMethod.Post, "aprobaciones/revision-semanal/firmar", dto);

    public Task<SeguridadEnvelope<List<TipoCambioContableDTO>>> TiposCambioAsync(string moneda, DateOnly desde, DateOnly hasta)
        => EnviarAsync<List<TipoCambioContableDTO>>(HttpMethod.Get, $"tipos-cambio?moneda={Uri.EscapeDataString(moneda)}&desde={Fecha(desde)}&hasta={Fecha(hasta)}");

    public Task<SeguridadEnvelope<TipoCambioContableDTO>> RegistrarTipoCambioAsync(RegistrarTipoCambioDTO dto)
        => EnviarAsync<TipoCambioContableDTO>(HttpMethod.Post, "tipos-cambio", dto);

    public Task<SeguridadEnvelope<List<RevaluacionDTO>>> RevaluacionesAsync(long idLibro)
        => EnviarAsync<List<RevaluacionDTO>>(HttpMethod.Get, $"revaluaciones?idLibroContable={idLibro}");

    public Task<SeguridadEnvelope<RevaluacionDTO>> SimularRevaluacionAsync(EjecutarRevaluacionDTO dto)
        => EnviarAsync<RevaluacionDTO>(HttpMethod.Post, "revaluaciones/simular", dto);

    public Task<SeguridadEnvelope<RevaluacionDTO>> EjecutarRevaluacionAsync(EjecutarRevaluacionDTO dto)
        => EnviarAsync<RevaluacionDTO>(HttpMethod.Post, "revaluaciones/ejecutar", dto);
}
