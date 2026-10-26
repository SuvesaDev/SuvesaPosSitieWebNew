using EjecucionConciliacionDTO = SuvesaPosSitioAplicacion.DTOs.Generated.EjecucionConciliacionDTO;
using ImportacionSaldosInicialesDTO = SuvesaPosSitioAplicacion.DTOs.Generated.ImportacionSaldosInicialesDTO;
using EvidenciaCierreDTO = SuvesaPosSitioAplicacion.DTOs.Generated.EvidenciaCierreDTO;
using SuvesaPosSitioAplicacion.DTOs.Contabilidad;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Contabilidad — Ola 2: aprobaciones y roles (F2), tipos de cambio y revaluación (F4).</summary>
public interface IAprobaciones
{
    Task<ResponseGeneric<ConfiguracionAprobacionDTO>> Configuracion(long idLibro);
    Task<ResponseGeneric<bool>> CambiarModo(long idLibro, string modo);
    Task<ResponseGeneric<bool>> AsignarRol(long idLibro, string usuario, string rol);
    Task<ResponseGeneric<bool>> QuitarRol(long idRol);
    Task<ResponseGeneric<SolicitudAprobacionDTO>> Solicitar(SolicitarAprobacionDTO comando);
    Task<ResponseGeneric<ICollection<SolicitudAprobacionDTO>>> Solicitudes(long idLibro, string? estado = null);
    Task<ResponseGeneric<SolicitudAprobacionDTO>> Resolver(long idSolicitud, bool aprobar, string? comentario, string? contrasena);
    Task<ResponseGeneric<bool>> Cancelar(long idSolicitud);
    Task<ResponseGeneric<RevisionSemanalDTO>> RevisionSemanal(long idLibro, DateOnly dia);
    Task<ResponseGeneric<RevisionSemanalDTO>> FirmarRevisionSemanal(long idLibro, DateOnly semanaInicio, string? comentario);

    Task<ResponseGeneric<ICollection<TipoCambioContableDTO>>> TiposCambio(string moneda, DateOnly desde, DateOnly hasta);
    Task<ResponseGeneric<TipoCambioContableDTO>> RegistrarTipoCambio(RegistrarTipoCambioDTO comando);
    Task<ResponseGeneric<ICollection<RevaluacionDTO>>> Revaluaciones(long idLibro);
    Task<ResponseGeneric<RevaluacionDTO>> SimularRevaluacion(long idLibro, int ejercicio, int mes, string moneda);
    Task<ResponseGeneric<RevaluacionDTO>> EjecutarRevaluacion(long idLibro, int ejercicio, int mes, string moneda);

    // ---- Ola 3 ----
    Task<ResponseGeneric<ResultadoSincronizacionCatalogoDTO>> SincronizarCatalogo(long idLibro);
    Task<ResponseGeneric<ICollection<UsoCuentaContableDTO>>> UsosCuenta(long idLibro);
    Task<ResponseGeneric<bool>> AsignarUsoCuenta(long idLibro, string uso, long idCuenta);
    Task<ResponseGeneric<ResumenProcesadoresDTO>> ResumenProcesadores(long idLibro);
    Task<ResponseGeneric<LiquidacionProcesadorDTO>> RegistrarLiquidacion(RegistrarLiquidacionProcesadorDTO comando);
    Task<ResponseGeneric<ICollection<CorteEntregaDTO>>> CortesEntrega(long idLibro);
    Task<ResponseGeneric<CorteEntregaDTO>> SimularCorteEntrega(long idLibro, int ejercicio, int mes);
    Task<ResponseGeneric<CorteEntregaDTO>> EjecutarCorteEntrega(long idLibro, int ejercicio, int mes);
    Task<ResponseGeneric<ICollection<GastoDTO>>> Gastos(long idLibro, string? estado = null);
    Task<ResponseGeneric<GastoDTO>> RegistrarGasto(RegistrarGastoDTO comando);
    Task<ResponseGeneric<GastoDTO>> ProgramarPagoGasto(long idGasto, DateOnly fecha);
    Task<ResponseGeneric<GastoDTO>> PagarGasto(long idGasto, DateOnly fecha);
    Task<ResponseGeneric<GastoDTO>> AnularGasto(long idGasto, string motivo);
    Task<ResponseGeneric<EstadoTablaRetencionesDTO>> EstadoTablaRetenciones();
    Task<ResponseGeneric<bool>> DescartarEvento(long idEvento, string estado, string motivo);
    Task<ResponseGeneric<ICollection<EstatusContableDocumentoDTO>>> EstatusDocumento(string modulo, string id);
    Task<ResponseGeneric<ICollection<DiferenciaCajaDTO>>> DiferenciasCaja(string? estado = null);
    Task<ResponseGeneric<DiferenciaCajaDTO>> ResolverDiferenciaCaja(long id, string modo, string motivo);
    Task<ResponseGeneric<ICollection<LineaNoDeducibleDTO>>> NoDeducibles(long idLibro, DateOnly desde, DateOnly hasta);
    Task<ResponseGeneric<bool>> DeclararDeducible(long idLibro, long idAsiento, bool esDeducible, string motivo);
    Task<ResponseGeneric<ReclasificacionInventarioDTO>> SimularReclasificacionInventario(long idLibro, DateOnly fechaCorte);
    Task<ResponseGeneric<ReclasificacionInventarioDTO>> EjecutarReclasificacionInventario(long idLibro, DateOnly fechaCorte);
    Task<ResponseGeneric<EjecucionConciliacionDTO>> ConciliarContraMayor(long idLibro, DateOnly fechaCorte);
    Task<ResponseGeneric<BalanzaPeriodoDTO>> BalanzaPeriodo(long idLibro, DateOnly desde, DateOnly hasta);
    Task<ResponseGeneric<EstadoResultadosNiifDTO>> ResultadosNiif(long idLibro, DateOnly desde, DateOnly hasta, bool reexpresado);
    Task<ResponseGeneric<SituacionFinancieraNiifDTO>> SituacionNiif(long idLibro, DateOnly fechaCorte, bool reexpresado);
    Task<ResponseGeneric<FlujoEfectivoIndirectoDTO>> FlujoIndirecto(long idLibro, DateOnly desde, DateOnly hasta);
    Task<ResponseGeneric<EstadoCambiosPatrimonioDTO>> CambiosPatrimonio(long idLibro, DateOnly desde, DateOnly hasta);
    Task<ResponseGeneric<ComparativoNiifDTO>> ComparativoNiif(long idLibro, int ejercicio);
    Task<ResponseGeneric<ICollection<ImportacionSaldosInicialesDTO>>> SaldosIniciales(long idLibro);
    Task<ResponseGeneric<ImportacionSaldosInicialesDTO>> CargarSaldosIniciales(long idLibro, DateOnly fechaCorte, Stream contenido, string nombreArchivo);
    Task<ResponseGeneric<ImportacionSaldosInicialesDTO>> ObtenerSaldosIniciales(long idImportacion);
    Task<ResponseGeneric<bool>> ConfirmarSaldosIniciales(long idImportacion, string? contrasena);
    Task<ResponseGeneric<bool>> RechazarSaldosIniciales(long idImportacion, string motivo);
    Task<ResponseGeneric<EjecucionConciliacionDTO>> CompararParalelo(long idLibro, DateOnly fechaCorte, Dictionary<string, decimal> saldosReferencia);
    Task<ResponseGeneric<PuestaEnMarchaDTO>> PuestaEnMarcha(long idLibro);
    Task<ResponseGeneric<PuestaEnMarchaDTO>> IniciarSombra(long idLibro, int idSucursalPiloto);
    Task<ResponseGeneric<PuestaEnMarchaDTO>> PasarAOficial(long idLibro, string? contrasena);
    Task<ResponseGeneric<ICollection<CasoUatDTO>>> CasosUat(long idLibro);
    Task<ResponseGeneric<CasoUatDTO>> ResultadoUat(long idCaso, string estado, string? comentario);
    Task<ResponseGeneric<FirmaUatDTO>> FirmarUat(long idLibro, string? comentario, string? contrasena);
    Task<ResponseGeneric<CorteAlbaranDTO>> SimularCorteAlbaranes(long idLibro, int ejercicio, int mes);
    Task<ResponseGeneric<CorteAlbaranDTO>> EjecutarCorteAlbaranes(long idLibro, int ejercicio, int mes);
    Task<ResponseGeneric<ICollection<EvidenciaCierreDTO>>> EvidenciasUat(long idCaso);
    Task<ResponseGeneric<EvidenciaCierreDTO>> AdjuntarEvidenciaUat(long idCaso, Stream contenido, string nombreArchivo, string contentType);
}
