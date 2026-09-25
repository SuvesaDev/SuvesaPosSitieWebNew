using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Contabilidad General — W1 (feature flag, activación) + W2 (catálogo de
/// cuentas, dimensiones y plantillas versionadas). El resto de <c>api/contabilidad/*</c>
/// se agrega proxy por proxy en fases posteriores, no todo de una vez.</summary>
public interface IContabilidad
{
    Task<ResponseGeneric<EstadoActivacionContabilidadDTO>> EstadoActivacion(long idEmpresa, int? idEmisor);
    Task<ResponseGeneric<bool>> ActivarEmpresa(long idEmpresa);
    Task<ResponseGeneric<bool>> DesactivarEmpresa(long idEmpresa);
    Task<ResponseGeneric<long>> ActivarEmisor(ActivarEmisorDTO comando);
    Task<ResponseGeneric<bool>> DesactivarEmisor(int idEmisor);
    Task<ResponseGeneric<bool>> ConfirmarClave(string contrasena);

    // ---- W2: catálogo de cuentas, dimensiones y plantillas ----
    Task<ResponseGeneric<ICollection<CuentaContableDTO>>> ListarCuentas(long idLibroContable);
    Task<ResponseGeneric<CuentaContableDTO>> CrearCuenta(CrearCuentaContableDTO comando);
    Task<ResponseGeneric<CuentaContableDTO>> EditarCuenta(long idCuenta, EditarCuentaContableDTO comando);

    Task<ResponseGeneric<ICollection<DimensionContableDTO>>> ListarDimensiones(long idLibroContable);
    Task<ResponseGeneric<long>> CrearDimension(CrearDimensionContableDTO comando);
    Task<ResponseGeneric<ICollection<ValorDimensionContableDTO>>> ListarValoresDimension(long idDimension);
    Task<ResponseGeneric<long>> CrearValorDimension(CrearValorDimensionDTO comando);

    Task<ResponseGeneric<ICollection<PlantillaContableDTO>>> ListarPlantillas(long idLibroContable);
    Task<ResponseGeneric<long>> CrearPlantilla(CrearPlantillaContableDTO comando);
    Task<ResponseGeneric<ICollection<VersionPlantillaDTO>>> ListarVersiones(long idPlantilla);
    Task<ResponseGeneric<VersionPlantillaDetalleDTO>> ObtenerVersion(long idVersion);
    Task<ResponseGeneric<VersionPlantillaDTO>> CrearVersion(long idPlantilla, CrearVersionPlantillaDTO comando);
    Task<ResponseGeneric<VersionPlantillaDTO>> ActivarVersion(long idVersion);
    Task<ResponseGeneric<ResultadoSimulacionDTO>> Simular(long idVersion, string payloadJson);

    // ---- W3: bandeja, pólizas manuales, reverso y reproceso ----
    Task<ResponseGeneric<ICollection<EventoContableDTO>>> ListarEventos(long? idLibroContable, string? estado, string? tipoEvento, string? origenModulo, DateOnly? desde, DateOnly? hasta, int pagina, int tamanoPagina);
    Task<ResponseGeneric<bool>> ReintentarEvento(long idEvento);
    Task<ResponseGeneric<ICollection<AsientoContableDTO>>> ListarAsientos(long? idLibroContable, int pagina, int tamanoPagina, long? idCliente = null, long? idProveedor = null, string? origenModulo = null);
    Task<ResponseGeneric<AsientoContableDTO>> CrearAsientoManual(CrearAsientoManualDTO comando);
    Task<ResponseGeneric<AsientoContableDTO>> ReversarAsiento(long idAsiento, string? contrasena);

    Task<ResponseGeneric<EjecucionRepolinizacionDTO>> SimularRepolinizacion(FiltroRepolinizacionDTO filtro);
    Task<ResponseGeneric<EjecucionRepolinizacionDTO>> ObtenerRepolinizacion(long idEjecucion);
    Task<ResponseGeneric<EjecucionRepolinizacionDTO>> AprobarRepolinizacion(long idEjecucion, string contrasena);

    // ---- W4: diario, mayor, auxiliares y conciliación ----
    Task<ResponseGeneric<ICollection<PeriodoContableDTO>>> ListarPeriodos(long idLibroContable);
    Task<ResponseGeneric<ICollection<AuditoriaContableDTO>>> ListarAuditoria(long? idLibroContable, string? entidad, string? usuario, DateOnly? desde, DateOnly? hasta, int pagina, int tamanoPagina);
    Task<ResponseGeneric<ICollection<AsientoContableDTO>>> ObtenerLibroDiario(long idLibroContable, DateOnly desde, DateOnly hasta);
    Task<ResponseGeneric<LibroMayorDTO>> ObtenerLibroMayor(long idCuentaContable, DateOnly desde, DateOnly hasta);

    Task<ResponseGeneric<EjecucionConciliacionDTO>> ConciliarCxC(int idEmisor);
    Task<ResponseGeneric<EjecucionConciliacionDTO>> ConciliarCxP(int idEmisor);
    Task<ResponseGeneric<EjecucionConciliacionDTO>> ConciliarInventario(long idLibroContable);
    Task<ResponseGeneric<EjecucionConciliacionDTO>> ObtenerConciliacion(long idEjecucion);
    Task<ResponseGeneric<bool>> ResolverDiferencia(long idDiferencia, string motivo, string? contrasena);
}
