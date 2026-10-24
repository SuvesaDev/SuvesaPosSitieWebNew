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
}
