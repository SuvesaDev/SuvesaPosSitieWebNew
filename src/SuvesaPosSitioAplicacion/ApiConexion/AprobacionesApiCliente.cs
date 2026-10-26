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

    Task<SeguridadEnvelope<ResultadoSincronizacionCatalogoDTO>> SincronizarCatalogoAsync(long idLibro);
    Task<SeguridadEnvelope<List<UsoCuentaContableDTO>>> UsosCuentaAsync(long idLibro);
    Task<SeguridadEnvelope<bool>> AsignarUsoCuentaAsync(AsignarUsoCuentaDTO dto);
    Task<SeguridadEnvelope<ResumenProcesadoresDTO>> ResumenProcesadoresAsync(long idLibro);
    Task<SeguridadEnvelope<LiquidacionProcesadorDTO>> RegistrarLiquidacionAsync(RegistrarLiquidacionProcesadorDTO dto);
    Task<SeguridadEnvelope<List<CorteEntregaDTO>>> CortesEntregaAsync(long idLibro);
    Task<SeguridadEnvelope<CorteEntregaDTO>> SimularCorteEntregaAsync(EjecutarCorteEntregaDTO dto);
    Task<SeguridadEnvelope<CorteEntregaDTO>> EjecutarCorteEntregaAsync(EjecutarCorteEntregaDTO dto);
    Task<SeguridadEnvelope<List<GastoDTO>>> GastosAsync(long idLibro, string? estado);
    Task<SeguridadEnvelope<GastoDTO>> RegistrarGastoAsync(RegistrarGastoDTO dto);
    Task<SeguridadEnvelope<GastoDTO>> ProgramarPagoGastoAsync(long idGasto, FechaGastoDTO dto);
    Task<SeguridadEnvelope<GastoDTO>> PagarGastoAsync(long idGasto, FechaGastoDTO dto);
    Task<SeguridadEnvelope<GastoDTO>> AnularGastoAsync(long idGasto, AnularGastoDTO dto);
    Task<SeguridadEnvelope<EstadoTablaRetencionesDTO>> EstadoTablaRetencionesAsync();
    Task<SeguridadEnvelope<bool>> DescartarEventoAsync(long idEvento, DescartarEventoDTO dto);
    Task<SeguridadEnvelope<List<EstatusContableDocumentoDTO>>> EstatusDocumentoAsync(string modulo, string id);
    Task<SeguridadEnvelope<List<DiferenciaCajaDTO>>> DiferenciasCajaAsync(string? estado);
    Task<SeguridadEnvelope<DiferenciaCajaDTO>> ResolverDiferenciaCajaAsync(long id, ResolverDiferenciaCajaDTO dto);
    Task<SeguridadEnvelope<List<LineaNoDeducibleDTO>>> NoDeduciblesAsync(long idLibro, DateOnly desde, DateOnly hasta);
    Task<SeguridadEnvelope<bool>> DeclararDeducibleAsync(DeclararDeducibleDTO dto);
    Task<SeguridadEnvelope<ReclasificacionInventarioDTO>> SimularReclasificacionInventarioAsync(EjecutarReclasificacionDTO dto);
    Task<SeguridadEnvelope<ReclasificacionInventarioDTO>> EjecutarReclasificacionInventarioAsync(EjecutarReclasificacionDTO dto);
    Task<SeguridadEnvelope<EjecucionConciliacionDTO>> ConciliarContraMayorAsync(long idLibro, DateOnly fechaCorte);
    Task<SeguridadEnvelope<BalanzaPeriodoDTO>> BalanzaPeriodoAsync(long idLibro, DateOnly desde, DateOnly hasta);
    Task<SeguridadEnvelope<EstadoResultadosNiifDTO>> ResultadosNiifAsync(long idLibro, DateOnly desde, DateOnly hasta, bool reexpresado);
    Task<SeguridadEnvelope<SituacionFinancieraNiifDTO>> SituacionNiifAsync(long idLibro, DateOnly fechaCorte, bool reexpresado);
    Task<SeguridadEnvelope<FlujoEfectivoIndirectoDTO>> FlujoIndirectoAsync(long idLibro, DateOnly desde, DateOnly hasta);
    Task<SeguridadEnvelope<EstadoCambiosPatrimonioDTO>> CambiosPatrimonioAsync(long idLibro, DateOnly desde, DateOnly hasta);
    Task<SeguridadEnvelope<ComparativoNiifDTO>> ComparativoNiifAsync(long idLibro, int ejercicio);

    Task<SeguridadEnvelope<List<ImportacionSaldosInicialesDTO>>> SaldosInicialesAsync(long idLibro);
    Task<SeguridadEnvelope<ImportacionSaldosInicialesDTO>> CargarSaldosInicialesAsync(long idLibro, DateOnly fechaCorte, Stream contenido, string nombreArchivo);
    Task<SeguridadEnvelope<ImportacionSaldosInicialesDTO>> ObtenerSaldosInicialesAsync(long idImportacion);
    Task<SeguridadEnvelope<AsientoContableDTO>> ConfirmarSaldosInicialesAsync(long idImportacion, ConfirmarSaldosInicialesDTO dto);
    Task<SeguridadEnvelope<bool>> RechazarSaldosInicialesAsync(long idImportacion, RechazarSaldosInicialesDTO dto);
    Task<SeguridadEnvelope<EjecucionConciliacionDTO>> CompararParaleloAsync(ParaleloComandoDTO dto);

    Task<SeguridadEnvelope<PuestaEnMarchaDTO>> PuestaEnMarchaAsync(long idLibro);
    Task<SeguridadEnvelope<PuestaEnMarchaDTO>> IniciarSombraAsync(IniciarSombraDTO dto);
    Task<SeguridadEnvelope<PuestaEnMarchaDTO>> PasarAOficialAsync(PasarAOficialDTO dto);
    Task<SeguridadEnvelope<List<CasoUatDTO>>> CasosUatAsync(long idLibro);
    Task<SeguridadEnvelope<CasoUatDTO>> ResultadoUatAsync(long idCaso, RegistrarResultadoUatDTO dto);
    Task<SeguridadEnvelope<FirmaUatDTO>> FirmarUatAsync(FirmarUatDTO dto);
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

    /// <summary>Envía un archivo como formulario multipart (la carga de saldos iniciales).</summary>
    private async Task<SeguridadEnvelope<T>> EnviarArchivoAsync<T>(string ruta, Stream contenido, string nombreArchivo, string contentType = "application/octet-stream")
    {
        using var formulario = new MultipartFormDataContent();
        var archivo = new StreamContent(contenido);
        archivo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        formulario.Add(archivo, "archivo", nombreArchivo);
        using var req = new HttpRequestMessage(HttpMethod.Post, Base + ruta) { Content = formulario };
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

    public Task<SeguridadEnvelope<ResultadoSincronizacionCatalogoDTO>> SincronizarCatalogoAsync(long idLibro)
        => EnviarAsync<ResultadoSincronizacionCatalogoDTO>(HttpMethod.Post, $"libros/{idLibro}/sincronizar-catalogo");

    public Task<SeguridadEnvelope<List<UsoCuentaContableDTO>>> UsosCuentaAsync(long idLibro)
        => EnviarAsync<List<UsoCuentaContableDTO>>(HttpMethod.Get, $"libros/{idLibro}/usos-cuenta");

    public Task<SeguridadEnvelope<bool>> AsignarUsoCuentaAsync(AsignarUsoCuentaDTO dto)
        => EnviarAsync<bool>(HttpMethod.Put, "usos-cuenta", dto);

    public Task<SeguridadEnvelope<ResumenProcesadoresDTO>> ResumenProcesadoresAsync(long idLibro)
        => EnviarAsync<ResumenProcesadoresDTO>(HttpMethod.Get, $"procesadores/resumen?idLibroContable={idLibro}");

    public Task<SeguridadEnvelope<LiquidacionProcesadorDTO>> RegistrarLiquidacionAsync(RegistrarLiquidacionProcesadorDTO dto)
        => EnviarAsync<LiquidacionProcesadorDTO>(HttpMethod.Post, "procesadores/liquidaciones", dto);

    public Task<SeguridadEnvelope<List<CorteEntregaDTO>>> CortesEntregaAsync(long idLibro)
        => EnviarAsync<List<CorteEntregaDTO>>(HttpMethod.Get, $"cortes-entrega?idLibroContable={idLibro}");

    public Task<SeguridadEnvelope<CorteEntregaDTO>> SimularCorteEntregaAsync(EjecutarCorteEntregaDTO dto)
        => EnviarAsync<CorteEntregaDTO>(HttpMethod.Post, "cortes-entrega/simular", dto);

    public Task<SeguridadEnvelope<CorteEntregaDTO>> EjecutarCorteEntregaAsync(EjecutarCorteEntregaDTO dto)
        => EnviarAsync<CorteEntregaDTO>(HttpMethod.Post, "cortes-entrega/ejecutar", dto);

    public Task<SeguridadEnvelope<List<GastoDTO>>> GastosAsync(long idLibro, string? estado)
        => EnviarAsync<List<GastoDTO>>(HttpMethod.Get, $"gastos?idLibroContable={idLibro}" + (string.IsNullOrWhiteSpace(estado) ? "" : $"&estado={Uri.EscapeDataString(estado)}"));

    public Task<SeguridadEnvelope<GastoDTO>> RegistrarGastoAsync(RegistrarGastoDTO dto)
        => EnviarAsync<GastoDTO>(HttpMethod.Post, "gastos", dto);

    public Task<SeguridadEnvelope<GastoDTO>> ProgramarPagoGastoAsync(long idGasto, FechaGastoDTO dto)
        => EnviarAsync<GastoDTO>(HttpMethod.Post, $"gastos/{idGasto}/programar-pago", dto);

    public Task<SeguridadEnvelope<GastoDTO>> PagarGastoAsync(long idGasto, FechaGastoDTO dto)
        => EnviarAsync<GastoDTO>(HttpMethod.Post, $"gastos/{idGasto}/pagar", dto);

    public Task<SeguridadEnvelope<GastoDTO>> AnularGastoAsync(long idGasto, AnularGastoDTO dto)
        => EnviarAsync<GastoDTO>(HttpMethod.Post, $"gastos/{idGasto}/anular", dto);

    public Task<SeguridadEnvelope<EstadoTablaRetencionesDTO>> EstadoTablaRetencionesAsync()
        => EnviarAsync<EstadoTablaRetencionesDTO>(HttpMethod.Get, "retenciones/estado");

    public Task<SeguridadEnvelope<bool>> DescartarEventoAsync(long idEvento, DescartarEventoDTO dto)
        => EnviarAsync<bool>(HttpMethod.Post, $"eventos/{idEvento}/descartar", dto);

    public Task<SeguridadEnvelope<List<EstatusContableDocumentoDTO>>> EstatusDocumentoAsync(string modulo, string id)
        => EnviarAsync<List<EstatusContableDocumentoDTO>>(HttpMethod.Get, $"estatus-documento?modulo={Uri.EscapeDataString(modulo)}&id={Uri.EscapeDataString(id)}");

    public Task<SeguridadEnvelope<List<DiferenciaCajaDTO>>> DiferenciasCajaAsync(string? estado)
        => EnviarAsync<List<DiferenciaCajaDTO>>(HttpMethod.Get, "diferencias-caja" + (string.IsNullOrWhiteSpace(estado) ? "" : $"?estado={Uri.EscapeDataString(estado)}"));

    public Task<SeguridadEnvelope<DiferenciaCajaDTO>> ResolverDiferenciaCajaAsync(long id, ResolverDiferenciaCajaDTO dto)
        => EnviarAsync<DiferenciaCajaDTO>(HttpMethod.Post, $"diferencias-caja/{id}/resolver", dto);

    public Task<SeguridadEnvelope<List<LineaNoDeducibleDTO>>> NoDeduciblesAsync(long idLibro, DateOnly desde, DateOnly hasta)
        => EnviarAsync<List<LineaNoDeducibleDTO>>(HttpMethod.Get, $"deducibilidad?idLibroContable={idLibro}&desde={Fecha(desde)}&hasta={Fecha(hasta)}");

    public Task<SeguridadEnvelope<bool>> DeclararDeducibleAsync(DeclararDeducibleDTO dto)
        => EnviarAsync<bool>(HttpMethod.Post, "deducibilidad", dto);

    public Task<SeguridadEnvelope<ReclasificacionInventarioDTO>> SimularReclasificacionInventarioAsync(EjecutarReclasificacionDTO dto)
        => EnviarAsync<ReclasificacionInventarioDTO>(HttpMethod.Post, "reclasificacion-inventario/simular", dto);

    public Task<SeguridadEnvelope<ReclasificacionInventarioDTO>> EjecutarReclasificacionInventarioAsync(EjecutarReclasificacionDTO dto)
        => EnviarAsync<ReclasificacionInventarioDTO>(HttpMethod.Post, "reclasificacion-inventario/ejecutar", dto);

    public Task<SeguridadEnvelope<EjecucionConciliacionDTO>> ConciliarContraMayorAsync(long idLibro, DateOnly fechaCorte)
        => EnviarAsync<EjecucionConciliacionDTO>(HttpMethod.Post, $"conciliaciones/mayor?idLibroContable={idLibro}&fechaCorte={Fecha(fechaCorte)}");

    public Task<SeguridadEnvelope<BalanzaPeriodoDTO>> BalanzaPeriodoAsync(long idLibro, DateOnly desde, DateOnly hasta)
        => EnviarAsync<BalanzaPeriodoDTO>(HttpMethod.Get, $"estados-niif/balanza-periodo?idLibroContable={idLibro}&desde={Fecha(desde)}&hasta={Fecha(hasta)}");

    public Task<SeguridadEnvelope<EstadoResultadosNiifDTO>> ResultadosNiifAsync(long idLibro, DateOnly desde, DateOnly hasta, bool reexpresado)
        => EnviarAsync<EstadoResultadosNiifDTO>(HttpMethod.Get, $"estados-niif/resultados?idLibroContable={idLibro}&desde={Fecha(desde)}&hasta={Fecha(hasta)}&reexpresado={(reexpresado ? "true" : "false")}");

    public Task<SeguridadEnvelope<SituacionFinancieraNiifDTO>> SituacionNiifAsync(long idLibro, DateOnly fechaCorte, bool reexpresado)
        => EnviarAsync<SituacionFinancieraNiifDTO>(HttpMethod.Get, $"estados-niif/situacion?idLibroContable={idLibro}&fechaCorte={Fecha(fechaCorte)}&reexpresado={(reexpresado ? "true" : "false")}");

    public Task<SeguridadEnvelope<FlujoEfectivoIndirectoDTO>> FlujoIndirectoAsync(long idLibro, DateOnly desde, DateOnly hasta)
        => EnviarAsync<FlujoEfectivoIndirectoDTO>(HttpMethod.Get, $"estados-niif/flujo-indirecto?idLibroContable={idLibro}&desde={Fecha(desde)}&hasta={Fecha(hasta)}");

    public Task<SeguridadEnvelope<EstadoCambiosPatrimonioDTO>> CambiosPatrimonioAsync(long idLibro, DateOnly desde, DateOnly hasta)
        => EnviarAsync<EstadoCambiosPatrimonioDTO>(HttpMethod.Get, $"estados-niif/cambios-patrimonio?idLibroContable={idLibro}&desde={Fecha(desde)}&hasta={Fecha(hasta)}");

    public Task<SeguridadEnvelope<ComparativoNiifDTO>> ComparativoNiifAsync(long idLibro, int ejercicio)
        => EnviarAsync<ComparativoNiifDTO>(HttpMethod.Get, $"estados-niif/comparativo?idLibroContable={idLibro}&ejercicio={ejercicio}");

    public Task<SeguridadEnvelope<List<ImportacionSaldosInicialesDTO>>> SaldosInicialesAsync(long idLibro)
        => EnviarAsync<List<ImportacionSaldosInicialesDTO>>(HttpMethod.Get, $"dia-cero/saldos-iniciales?idLibroContable={idLibro}");

    public Task<SeguridadEnvelope<ImportacionSaldosInicialesDTO>> CargarSaldosInicialesAsync(long idLibro, DateOnly fechaCorte, Stream contenido, string nombreArchivo)
        => EnviarArchivoAsync<ImportacionSaldosInicialesDTO>($"dia-cero/saldos-iniciales?idLibroContable={idLibro}&fechaCorte={Fecha(fechaCorte)}", contenido, nombreArchivo);

    public Task<SeguridadEnvelope<ImportacionSaldosInicialesDTO>> ObtenerSaldosInicialesAsync(long idImportacion)
        => EnviarAsync<ImportacionSaldosInicialesDTO>(HttpMethod.Get, $"dia-cero/saldos-iniciales/{idImportacion}");

    public Task<SeguridadEnvelope<AsientoContableDTO>> ConfirmarSaldosInicialesAsync(long idImportacion, ConfirmarSaldosInicialesDTO dto)
        => EnviarAsync<AsientoContableDTO>(HttpMethod.Post, $"dia-cero/saldos-iniciales/{idImportacion}/confirmar", dto);

    public Task<SeguridadEnvelope<bool>> RechazarSaldosInicialesAsync(long idImportacion, RechazarSaldosInicialesDTO dto)
        => EnviarAsync<bool>(HttpMethod.Post, $"dia-cero/saldos-iniciales/{idImportacion}/rechazar", dto);

    public Task<SeguridadEnvelope<EjecucionConciliacionDTO>> CompararParaleloAsync(ParaleloComandoDTO dto)
        => EnviarAsync<EjecucionConciliacionDTO>(HttpMethod.Post, "dia-cero/paralelo", dto);

    public Task<SeguridadEnvelope<PuestaEnMarchaDTO>> PuestaEnMarchaAsync(long idLibro)
        => EnviarAsync<PuestaEnMarchaDTO>(HttpMethod.Get, $"puesta-en-marcha?idLibroContable={idLibro}");

    public Task<SeguridadEnvelope<PuestaEnMarchaDTO>> IniciarSombraAsync(IniciarSombraDTO dto)
        => EnviarAsync<PuestaEnMarchaDTO>(HttpMethod.Post, "puesta-en-marcha/sombra", dto);

    public Task<SeguridadEnvelope<PuestaEnMarchaDTO>> PasarAOficialAsync(PasarAOficialDTO dto)
        => EnviarAsync<PuestaEnMarchaDTO>(HttpMethod.Post, "puesta-en-marcha/oficial", dto);

    public Task<SeguridadEnvelope<List<CasoUatDTO>>> CasosUatAsync(long idLibro)
        => EnviarAsync<List<CasoUatDTO>>(HttpMethod.Get, $"puesta-en-marcha/uat?idLibroContable={idLibro}");

    public Task<SeguridadEnvelope<CasoUatDTO>> ResultadoUatAsync(long idCaso, RegistrarResultadoUatDTO dto)
        => EnviarAsync<CasoUatDTO>(HttpMethod.Post, $"puesta-en-marcha/uat/{idCaso}/resultado", dto);

    public Task<SeguridadEnvelope<FirmaUatDTO>> FirmarUatAsync(FirmarUatDTO dto)
        => EnviarAsync<FirmaUatDTO>(HttpMethod.Post, "puesta-en-marcha/uat/firmar", dto);
}
