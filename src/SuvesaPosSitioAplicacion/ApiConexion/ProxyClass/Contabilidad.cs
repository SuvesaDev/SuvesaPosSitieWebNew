using System.IO;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IContabilidad" />
public sealed class Contabilidad : ProxyBase, IContabilidad
{
    private readonly IContabilidadApiCliente _api;

    public Contabilidad(
        IContabilidadApiCliente api,
        IContextoSesion sesion,
        ILogger<Contabilidad> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<EstadoActivacionContabilidadDTO>> EstadoActivacion(long idEmpresa, int? idEmisor)
        => Ejecutar(async () =>
        {
            var r = await _api.EstadoActivacionAsync(idEmpresa, idEmisor);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el estado de activación de Contabilidad");

    public Task<ResponseGeneric<bool>> ActivarEmpresa(long idEmpresa)
        => Ejecutar(async () =>
        {
            var r = await _api.ActivarAsync(idEmpresa);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "activar Contabilidad para la empresa");

    public Task<ResponseGeneric<bool>> DesactivarEmpresa(long idEmpresa)
        => Ejecutar(async () =>
        {
            var r = await _api.DesactivarAsync(idEmpresa);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "desactivar Contabilidad para la empresa");

    public Task<ResponseGeneric<long>> ActivarEmisor(ActivarEmisorDTO comando)
        => Ejecutar(async () =>
        {
            var r = await _api.Activar2Async(comando);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "activar Contabilidad para el emisor");

    public Task<ResponseGeneric<bool>> DesactivarEmisor(int idEmisor)
        => Ejecutar(async () =>
        {
            var r = await _api.Desactivar2Async(idEmisor);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "desactivar Contabilidad para el emisor");

    public Task<ResponseGeneric<bool>> ConfirmarClave(string contrasena)
        => Ejecutar(async () =>
        {
            var r = await _api.ConfirmarClaveAsync(new ConfirmarClaveDTO { Contrasena = contrasena });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "confirmar la contraseña");

    // ---- W2: catálogo de cuentas, dimensiones y plantillas ----

    public Task<ResponseGeneric<ICollection<CuentaContableDTO>>> ListarCuentas(long idLibroContable)
        => Ejecutar(async () =>
        {
            var r = await _api.CuentasGETAsync(idLibroContable);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el catálogo de cuentas");

    public Task<ResponseGeneric<CuentaContableDTO>> CrearCuenta(CrearCuentaContableDTO comando)
        => Ejecutar(async () =>
        {
            var r = await _api.CuentasPOSTAsync(comando);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la cuenta contable");

    public Task<ResponseGeneric<CuentaContableDTO>> EditarCuenta(long idCuenta, EditarCuentaContableDTO comando)
        => Ejecutar(async () =>
        {
            var r = await _api.CuentasPUTAsync(idCuenta, comando);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar la cuenta contable");

    public Task<ResponseGeneric<ICollection<DimensionContableDTO>>> ListarDimensiones(long idLibroContable)
        => Ejecutar(async () =>
        {
            var r = await _api.DimensionesGETAsync(idLibroContable);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las dimensiones contables");

    public Task<ResponseGeneric<long>> CrearDimension(CrearDimensionContableDTO comando)
        => Ejecutar(async () =>
        {
            var r = await _api.DimensionesPOSTAsync(comando);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la dimensión contable");

    public Task<ResponseGeneric<ICollection<ValorDimensionContableDTO>>> ListarValoresDimension(long idDimension)
        => Ejecutar(async () =>
        {
            var r = await _api.ValoresGETAsync(idDimension);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los valores de la dimensión");

    public Task<ResponseGeneric<long>> CrearValorDimension(CrearValorDimensionDTO comando)
        => Ejecutar(async () =>
        {
            var r = await _api.ValoresPOSTAsync(comando);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear el valor de dimensión");

    public Task<ResponseGeneric<ICollection<PlantillaContableDTO>>> ListarPlantillas(long idLibroContable)
        => Ejecutar(async () =>
        {
            var r = await _api.PlantillasGETAsync(idLibroContable);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las plantillas contables");

    public Task<ResponseGeneric<long>> CrearPlantilla(CrearPlantillaContableDTO comando)
        => Ejecutar(async () =>
        {
            var r = await _api.PlantillasPOSTAsync(comando);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la plantilla contable");

    public Task<ResponseGeneric<ICollection<VersionPlantillaDTO>>> ListarVersiones(long idPlantilla)
        => Ejecutar(async () =>
        {
            var r = await _api.VersionesGETAsync(idPlantilla);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las versiones de la plantilla");

    public Task<ResponseGeneric<VersionPlantillaDetalleDTO>> ObtenerVersion(long idVersion)
        => Ejecutar(async () =>
        {
            var r = await _api.VersionesGET2Async(idVersion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el detalle de la versión");

    public Task<ResponseGeneric<VersionPlantillaDTO>> CrearVersion(long idPlantilla, CrearVersionPlantillaDTO comando)
        => Ejecutar(async () =>
        {
            var r = await _api.VersionesPOSTAsync(idPlantilla, comando);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la versión de la plantilla");

    public Task<ResponseGeneric<VersionPlantillaDTO>> ActivarVersion(long idVersion)
        => Ejecutar(async () =>
        {
            var r = await _api.Activar3Async(idVersion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "activar la versión de la plantilla");

    public Task<ResponseGeneric<ResultadoSimulacionDTO>> Simular(long idVersion, string payloadJson)
        => Ejecutar(async () =>
        {
            var r = await _api.SimularAsync(idVersion, new SimularPlantillaDTO { PayloadJson = payloadJson });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "simular la versión de la plantilla");

    // ---- W3: bandeja, pólizas manuales, reverso y reproceso ----

    public Task<ResponseGeneric<ICollection<EventoContableDTO>>> ListarEventos(long? idLibroContable, string? estado, string? tipoEvento, string? origenModulo, DateOnly? desde, DateOnly? hasta, int pagina, int tamanoPagina)
        => Ejecutar(async () =>
        {
            var r = await _api.EventosAsync(idLibroContable, estado, tipoEvento, origenModulo, AFecha(desde), AFecha(hasta), pagina, tamanoPagina);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la bandeja de eventos");

    public Task<ResponseGeneric<bool>> ReintentarEvento(long idEvento)
        => Ejecutar(async () =>
        {
            var r = await _api.Reintentar2Async(idEvento);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "reintentar el evento contable");

    public Task<ResponseGeneric<ICollection<AsientoContableDTO>>> ListarAsientos(long? idLibroContable, int pagina, int tamanoPagina, long? idCliente = null, long? idProveedor = null, string? origenModulo = null)
        => Ejecutar(async () =>
        {
            var r = await _api.AsientosAsync(idLibroContable, pagina, tamanoPagina, idCliente, idProveedor, origenModulo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la bandeja de pólizas");

    public Task<ResponseGeneric<AsientoContableDTO>> CrearAsientoManual(CrearAsientoManualDTO comando)
        => Ejecutar(async () =>
        {
            var r = await _api.AsientosManualesAsync(comando);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear la póliza manual");

    public Task<ResponseGeneric<AsientoContableDTO>> ReversarAsiento(long idAsiento, string? contrasena)
        => Ejecutar(async () =>
        {
            var r = await _api.ReversarAsync(idAsiento, new ReversarAsientoDTO { Contrasena = contrasena });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "reversar la póliza");

    public Task<ResponseGeneric<EjecucionRepolinizacionDTO>> SimularRepolinizacion(FiltroRepolinizacionDTO filtro)
        => Ejecutar(async () =>
        {
            var r = await _api.Simular2Async(filtro);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "simular el reproceso");

    public Task<ResponseGeneric<EjecucionRepolinizacionDTO>> ObtenerRepolinizacion(long idEjecucion)
        => Ejecutar(async () =>
        {
            var r = await _api.RepolinizacionAsync(idEjecucion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la ejecución de reproceso");

    public Task<ResponseGeneric<EjecucionRepolinizacionDTO>> AprobarRepolinizacion(long idEjecucion, string contrasena)
        => Ejecutar(async () =>
        {
            var r = await _api.AprobarAsync(idEjecucion, new AprobarRepolinizacionDTO { Contrasena = contrasena });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "aprobar el reproceso");

    // ---- W4: diario, mayor, auxiliares y conciliación ----

    public Task<ResponseGeneric<ICollection<PeriodoContableDTO>>> ListarPeriodos(long idLibroContable)
        => Ejecutar(async () =>
        {
            var r = await _api.PeriodosGETAsync(idLibroContable);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los períodos contables");

    public Task<ResponseGeneric<ICollection<AuditoriaContableDTO>>> ListarAuditoria(long? idLibroContable, string? entidad, string? usuario, DateOnly? desde, DateOnly? hasta, int pagina, int tamanoPagina)
        => Ejecutar(async () =>
        {
            var r = await _api.AuditoriaAsync(idLibroContable, entidad, usuario, AFecha(desde), AFecha(hasta), pagina, tamanoPagina);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la bitácora de auditoría");

    public Task<ResponseGeneric<ICollection<AsientoContableDTO>>> ObtenerLibroDiario(long idLibroContable, DateOnly desde, DateOnly hasta)
        => Ejecutar(async () =>
        {
            var r = await _api.DiarioAsync(idLibroContable, AFecha(desde), AFecha(hasta));
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el libro diario");

    public Task<ResponseGeneric<LibroMayorDTO>> ObtenerLibroMayor(long idCuentaContable, DateOnly desde, DateOnly hasta)
        => Ejecutar(async () =>
        {
            var r = await _api.MayorAsync(idCuentaContable, AFecha(desde), AFecha(hasta));
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el libro mayor");

    public Task<ResponseGeneric<BalanzaComprobacionDTO>> ObtenerBalanza(long idLibroContable, DateOnly fechaCorte)
        => Ejecutar(async () =>
        {
            var r = await _api.BalanzaAsync(idLibroContable, AFecha(fechaCorte));
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la balanza de comprobación");

    public Task<ResponseGeneric<EstadoResultadosDTO>> ObtenerEstadoResultados(long idLibroContable, DateOnly desde, DateOnly hasta)
        => Ejecutar(async () =>
        {
            var r = await _api.EstadoResultadosAsync(idLibroContable, AFecha(desde), AFecha(hasta));
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el estado de resultados");

    public Task<ResponseGeneric<BalanceGeneralDTO>> ObtenerBalanceGeneral(long idLibroContable, DateOnly fechaCorte)
        => Ejecutar(async () =>
        {
            var r = await _api.BalanceGeneralAsync(idLibroContable, AFecha(fechaCorte));
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el balance general");

    public Task<ResponseGeneric<EstadoFlujoEfectivoDTO>> ObtenerEstadoFlujoEfectivo(long idLibroContable, DateOnly desde, DateOnly hasta)
        => Ejecutar(async () =>
        {
            var r = await _api.FlujoEfectivoAsync(idLibroContable, AFecha(desde), AFecha(hasta));
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el estado de flujo de efectivo");

    public Task<ResponseGeneric<EjecucionConciliacionDTO>> ConciliarCxC(int idEmisor)
        => Ejecutar(async () =>
        {
            var r = await _api.CxcPOSTAsync(idEmisor);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "conciliar el auxiliar de cuentas por cobrar");

    public Task<ResponseGeneric<EjecucionConciliacionDTO>> ConciliarCxP(int idEmisor)
        => Ejecutar(async () =>
        {
            var r = await _api.CxpAsync(idEmisor);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "conciliar el auxiliar de cuentas por pagar");

    public Task<ResponseGeneric<EjecucionConciliacionDTO>> ConciliarInventario(long idLibroContable)
        => Ejecutar(async () =>
        {
            var r = await _api.InventarioPOSTAsync(idLibroContable);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "conciliar el auxiliar de inventario");

    public Task<ResponseGeneric<EjecucionConciliacionDTO>> ObtenerConciliacion(long idEjecucion)
        => Ejecutar(async () =>
        {
            var r = await _api.CxcGETAsync(idEjecucion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la ejecución de conciliación");

    public Task<ResponseGeneric<bool>> ResolverDiferencia(long idDiferencia, string motivo, string? contrasena)
        => Ejecutar(async () =>
        {
            var r = await _api.ResolverAsync(idDiferencia, new ResolverDiferenciaDTO { Motivo = motivo, Contrasena = contrasena ?? string.Empty });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "resolver la diferencia de conciliación");

    // ---- W5: cierre mensual (A5, nunca conectado a Web hasta ahora) ----

    public Task<ResponseGeneric<ChecklistPreCierreDTO>> EjecutarPreCierre(long idPeriodo)
        => Ejecutar(async () =>
        {
            var r = await _api.PreCierreAsync(idPeriodo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "ejecutar el pre-cierre");

    public Task<ResponseGeneric<PeriodoContableDTO>> CerrarPeriodo(long idPeriodo)
        => Ejecutar(async () =>
        {
            var r = await _api.Cerrar2Async(idPeriodo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "cerrar el período");

    public Task<ResponseGeneric<PeriodoContableDTO>> BloquearPeriodo(long idPeriodo)
        => Ejecutar(async () =>
        {
            var r = await _api.BloquearAsync(idPeriodo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "bloquear el período");

    public Task<ResponseGeneric<ICollection<PeriodoContableDTO>>> ReabrirPeriodo(long idPeriodo, string motivo, string contrasena)
        => Ejecutar(async () =>
        {
            var r = await _api.ReabrirAsync(idPeriodo, new ReabrirPeriodoDTO { Motivo = motivo, Contrasena = contrasena });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "reabrir el período");

    // ---- W5: cierre anual y evidencia de adjunto ----

    public Task<ResponseGeneric<CierreAnualDTO>> EjecutarCierreAnual(long idLibroContable, int ejercicio, string contrasena)
        => Ejecutar(async () =>
        {
            var r = await _api.CierreAnualPOSTAsync(new EjecutarCierreAnualDTO { IdLibroContable = idLibroContable, Ejercicio = ejercicio, Contrasena = contrasena });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "ejecutar el cierre anual");

    public Task<ResponseGeneric<CierreAnualDTO>> ObtenerCierreAnual(long idLibroContable, int ejercicio)
        => Ejecutar(async () =>
        {
            var r = await _api.CierreAnualGETAsync(idLibroContable, ejercicio);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el cierre anual");

    public Task<ResponseGeneric<EvidenciaCierreDTO>> CargarEvidenciaCierre(long idLibroContable, string tipoOperacion, long idReferencia, Stream archivo, string nombreArchivo, string contentType)
        => Ejecutar(async () =>
        {
            var r = await _api.EvidenciasCierrePOSTAsync(idLibroContable, tipoOperacion, idReferencia, new FileParameter(archivo, nombreArchivo, contentType));
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "cargar la evidencia de cierre");

    public Task<ResponseGeneric<ICollection<EvidenciaCierreDTO>>> ListarEvidenciasCierre(string tipoOperacion, long idReferencia)
        => Ejecutar(async () =>
        {
            var r = await _api.EvidenciasCierreGETAsync(tipoOperacion, idReferencia);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar la evidencia de cierre");

    private static DateTimeOffset? AFecha(DateOnly? fecha) => fecha is null ? null : new DateTimeOffset(fecha.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
}
