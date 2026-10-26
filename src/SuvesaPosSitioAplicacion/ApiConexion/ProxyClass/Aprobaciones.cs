using EjecucionConciliacionDTO = SuvesaPosSitioAplicacion.DTOs.Generated.EjecucionConciliacionDTO;
using ImportacionSaldosInicialesDTO = SuvesaPosSitioAplicacion.DTOs.Generated.ImportacionSaldosInicialesDTO;
using EvidenciaCierreDTO = SuvesaPosSitioAplicacion.DTOs.Generated.EvidenciaCierreDTO;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Contabilidad;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IAprobaciones" />
public sealed class Aprobaciones : ProxyBase, IAprobaciones
{
    private readonly IAprobacionesApiCliente _api;

    public Aprobaciones(IAprobacionesApiCliente api, IContextoSesion sesion, ILogger<Aprobaciones> log)
        : base(sesion, log)
    {
        _api = api;
    }

    private static ResponseGeneric<T> Traducir<T>(SeguridadEnvelope<T> r) => EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);

    private static ResponseGeneric<ICollection<T>> TraducirLista<T>(SeguridadEnvelope<List<T>> r)
        => EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, (ICollection<T>?)r.Responses);

    public Task<ResponseGeneric<ConfiguracionAprobacionDTO>> Configuracion(long idLibro)
        => Ejecutar(async () => Traducir(await _api.ConfiguracionAsync(idLibro)), "consultar la configuración de aprobaciones");

    public Task<ResponseGeneric<bool>> CambiarModo(long idLibro, string modo)
        => Ejecutar(async () => Traducir(await _api.CambiarModoAsync(new CambiarModoAprobacionDTO { IdLibroContable = idLibro, Modo = modo })), "cambiar el modo de aprobación");

    public Task<ResponseGeneric<bool>> AsignarRol(long idLibro, string usuario, string rol)
        => Ejecutar(async () => Traducir(await _api.AsignarRolAsync(new AsignarRolContableDTO { IdLibroContable = idLibro, Usuario = usuario, Rol = rol })), "asignar el rol");

    public Task<ResponseGeneric<bool>> QuitarRol(long idRol)
        => Ejecutar(async () => Traducir(await _api.QuitarRolAsync(idRol)), "quitar el rol");

    public Task<ResponseGeneric<SolicitudAprobacionDTO>> Solicitar(SolicitarAprobacionDTO comando)
        => Ejecutar(async () => Traducir(await _api.SolicitarAsync(comando)), "enviar la solicitud");

    public Task<ResponseGeneric<ICollection<SolicitudAprobacionDTO>>> Solicitudes(long idLibro, string? estado = null)
        => Ejecutar<ICollection<SolicitudAprobacionDTO>>(async () => TraducirLista(await _api.SolicitudesAsync(idLibro, estado)), "consultar las solicitudes");

    public Task<ResponseGeneric<SolicitudAprobacionDTO>> Resolver(long idSolicitud, bool aprobar, string? comentario, string? contrasena)
        => Ejecutar(async () => Traducir(await _api.ResolverAsync(idSolicitud, new ResolverSolicitudDTO { Aprobar = aprobar, Comentario = comentario, Contrasena = contrasena })), "resolver la solicitud");

    public Task<ResponseGeneric<bool>> Cancelar(long idSolicitud)
        => Ejecutar(async () => Traducir(await _api.CancelarAsync(idSolicitud)), "cancelar la solicitud");

    public Task<ResponseGeneric<RevisionSemanalDTO>> RevisionSemanal(long idLibro, DateOnly dia)
        => Ejecutar(async () => Traducir(await _api.RevisionSemanalAsync(idLibro, dia)), "consultar la revisión semanal");

    public Task<ResponseGeneric<RevisionSemanalDTO>> FirmarRevisionSemanal(long idLibro, DateOnly semanaInicio, string? comentario)
        => Ejecutar(async () => Traducir(await _api.FirmarRevisionSemanalAsync(new FirmarRevisionSemanalDTO { IdLibroContable = idLibro, SemanaInicio = semanaInicio, Comentario = comentario })), "firmar la revisión semanal");

    public Task<ResponseGeneric<ICollection<TipoCambioContableDTO>>> TiposCambio(string moneda, DateOnly desde, DateOnly hasta)
        => Ejecutar<ICollection<TipoCambioContableDTO>>(async () => TraducirLista(await _api.TiposCambioAsync(moneda, desde, hasta)), "consultar los tipos de cambio");

    public Task<ResponseGeneric<TipoCambioContableDTO>> RegistrarTipoCambio(RegistrarTipoCambioDTO comando)
        => Ejecutar(async () => Traducir(await _api.RegistrarTipoCambioAsync(comando)), "registrar el tipo de cambio");

    public Task<ResponseGeneric<ICollection<RevaluacionDTO>>> Revaluaciones(long idLibro)
        => Ejecutar<ICollection<RevaluacionDTO>>(async () => TraducirLista(await _api.RevaluacionesAsync(idLibro)), "consultar las revaluaciones");

    public Task<ResponseGeneric<RevaluacionDTO>> SimularRevaluacion(long idLibro, int ejercicio, int mes, string moneda)
        => Ejecutar(async () => Traducir(await _api.SimularRevaluacionAsync(new EjecutarRevaluacionDTO { IdLibroContable = idLibro, Ejercicio = ejercicio, Mes = mes, Moneda = moneda })), "simular la revaluación");

    public Task<ResponseGeneric<RevaluacionDTO>> EjecutarRevaluacion(long idLibro, int ejercicio, int mes, string moneda)
        => Ejecutar(async () => Traducir(await _api.EjecutarRevaluacionAsync(new EjecutarRevaluacionDTO { IdLibroContable = idLibro, Ejercicio = ejercicio, Mes = mes, Moneda = moneda })), "ejecutar la revaluación");

    public Task<ResponseGeneric<ResultadoSincronizacionCatalogoDTO>> SincronizarCatalogo(long idLibro)
        => Ejecutar(async () => Traducir(await _api.SincronizarCatalogoAsync(idLibro)), "actualizar el catálogo");

    public Task<ResponseGeneric<ICollection<UsoCuentaContableDTO>>> UsosCuenta(long idLibro)
        => Ejecutar<ICollection<UsoCuentaContableDTO>>(async () => TraducirLista(await _api.UsosCuentaAsync(idLibro)), "consultar las cuentas de uso");

    public Task<ResponseGeneric<bool>> AsignarUsoCuenta(long idLibro, string uso, long idCuenta)
        => Ejecutar(async () => Traducir(await _api.AsignarUsoCuentaAsync(new AsignarUsoCuentaDTO { IdLibroContable = idLibro, Uso = uso, IdCuentaContable = idCuenta })), "asignar la cuenta de uso");

    public Task<ResponseGeneric<ResumenProcesadoresDTO>> ResumenProcesadores(long idLibro)
        => Ejecutar(async () => Traducir(await _api.ResumenProcesadoresAsync(idLibro)), "consultar los procesadores de pago");

    public Task<ResponseGeneric<LiquidacionProcesadorDTO>> RegistrarLiquidacion(RegistrarLiquidacionProcesadorDTO comando)
        => Ejecutar(async () => Traducir(await _api.RegistrarLiquidacionAsync(comando)), "registrar la liquidación");

    public Task<ResponseGeneric<ICollection<CorteEntregaDTO>>> CortesEntrega(long idLibro)
        => Ejecutar<ICollection<CorteEntregaDTO>>(async () => TraducirLista(await _api.CortesEntregaAsync(idLibro)), "consultar los cortes de entregas");

    public Task<ResponseGeneric<CorteEntregaDTO>> SimularCorteEntrega(long idLibro, int ejercicio, int mes)
        => Ejecutar(async () => Traducir(await _api.SimularCorteEntregaAsync(new EjecutarCorteEntregaDTO { IdLibroContable = idLibro, Ejercicio = ejercicio, Mes = mes })), "simular el corte de entregas");

    public Task<ResponseGeneric<CorteEntregaDTO>> EjecutarCorteEntrega(long idLibro, int ejercicio, int mes)
        => Ejecutar(async () => Traducir(await _api.EjecutarCorteEntregaAsync(new EjecutarCorteEntregaDTO { IdLibroContable = idLibro, Ejercicio = ejercicio, Mes = mes })), "ejecutar el corte de entregas");

    public Task<ResponseGeneric<ICollection<GastoDTO>>> Gastos(long idLibro, string? estado = null)
        => Ejecutar<ICollection<GastoDTO>>(async () => TraducirLista(await _api.GastosAsync(idLibro, estado)), "consultar los gastos");

    public Task<ResponseGeneric<GastoDTO>> RegistrarGasto(RegistrarGastoDTO comando)
        => Ejecutar(async () => Traducir(await _api.RegistrarGastoAsync(comando)), "registrar el gasto");

    public Task<ResponseGeneric<GastoDTO>> ProgramarPagoGasto(long idGasto, DateOnly fecha)
        => Ejecutar(async () => Traducir(await _api.ProgramarPagoGastoAsync(idGasto, new FechaGastoDTO { Fecha = fecha })), "programar el pago");

    public Task<ResponseGeneric<GastoDTO>> PagarGasto(long idGasto, DateOnly fecha)
        => Ejecutar(async () => Traducir(await _api.PagarGastoAsync(idGasto, new FechaGastoDTO { Fecha = fecha })), "pagar el gasto");

    public Task<ResponseGeneric<GastoDTO>> AnularGasto(long idGasto, string motivo)
        => Ejecutar(async () => Traducir(await _api.AnularGastoAsync(idGasto, new AnularGastoDTO { Motivo = motivo })), "anular el gasto");

    public Task<ResponseGeneric<EstadoTablaRetencionesDTO>> EstadoTablaRetenciones()
        => Ejecutar(async () => Traducir(await _api.EstadoTablaRetencionesAsync()), "consultar la tabla de retenciones");

    public Task<ResponseGeneric<bool>> DescartarEvento(long idEvento, string estado, string motivo)
        => Ejecutar(async () => Traducir(await _api.DescartarEventoAsync(idEvento, new DescartarEventoDTO { Estado = estado, Motivo = motivo })), "descartar el evento");

    public Task<ResponseGeneric<ICollection<EstatusContableDocumentoDTO>>> EstatusDocumento(string modulo, string id)
        => Ejecutar<ICollection<EstatusContableDocumentoDTO>>(async () => TraducirLista(await _api.EstatusDocumentoAsync(modulo, id)), "consultar el estatus contable");

    public Task<ResponseGeneric<ICollection<DiferenciaCajaDTO>>> DiferenciasCaja(string? estado = null)
        => Ejecutar<ICollection<DiferenciaCajaDTO>>(async () => TraducirLista(await _api.DiferenciasCajaAsync(estado)), "consultar las diferencias de caja");

    public Task<ResponseGeneric<DiferenciaCajaDTO>> ResolverDiferenciaCaja(long id, string modo, string motivo)
        => Ejecutar(async () => Traducir(await _api.ResolverDiferenciaCajaAsync(id, new ResolverDiferenciaCajaDTO { Modo = modo, Motivo = motivo })), "resolver la diferencia de caja");

    public Task<ResponseGeneric<ICollection<LineaNoDeducibleDTO>>> NoDeducibles(long idLibro, DateOnly desde, DateOnly hasta)
        => Ejecutar<ICollection<LineaNoDeducibleDTO>>(async () => TraducirLista(await _api.NoDeduciblesAsync(idLibro, desde, hasta)), "consultar los gastos no deducibles");

    public Task<ResponseGeneric<bool>> DeclararDeducible(long idLibro, long idAsiento, bool esDeducible, string motivo)
        => Ejecutar(async () => Traducir(await _api.DeclararDeducibleAsync(new DeclararDeducibleDTO { IdLibroContable = idLibro, IdAsiento = idAsiento, EsDeducible = esDeducible, Motivo = motivo })), "declarar la deducibilidad");

    public Task<ResponseGeneric<ReclasificacionInventarioDTO>> SimularReclasificacionInventario(long idLibro, DateOnly fechaCorte)
        => Ejecutar(async () => Traducir(await _api.SimularReclasificacionInventarioAsync(new EjecutarReclasificacionDTO { IdLibroContable = idLibro, FechaCorte = fechaCorte })), "simular la reclasificación del inventario");

    public Task<ResponseGeneric<ReclasificacionInventarioDTO>> EjecutarReclasificacionInventario(long idLibro, DateOnly fechaCorte)
        => Ejecutar(async () => Traducir(await _api.EjecutarReclasificacionInventarioAsync(new EjecutarReclasificacionDTO { IdLibroContable = idLibro, FechaCorte = fechaCorte })), "ejecutar la reclasificación del inventario");

    public Task<ResponseGeneric<EjecucionConciliacionDTO>> ConciliarContraMayor(long idLibro, DateOnly fechaCorte)
        => Ejecutar(async () => Traducir(await _api.ConciliarContraMayorAsync(idLibro, fechaCorte)), "conciliar contra el mayor");

    public Task<ResponseGeneric<BalanzaPeriodoDTO>> BalanzaPeriodo(long idLibro, DateOnly desde, DateOnly hasta)
        => Ejecutar(async () => Traducir(await _api.BalanzaPeriodoAsync(idLibro, desde, hasta)), "consultar la balanza del período");

    public Task<ResponseGeneric<EstadoResultadosNiifDTO>> ResultadosNiif(long idLibro, DateOnly desde, DateOnly hasta, bool reexpresado)
        => Ejecutar(async () => Traducir(await _api.ResultadosNiifAsync(idLibro, desde, hasta, reexpresado)), "consultar el estado de resultados");

    public Task<ResponseGeneric<SituacionFinancieraNiifDTO>> SituacionNiif(long idLibro, DateOnly fechaCorte, bool reexpresado)
        => Ejecutar(async () => Traducir(await _api.SituacionNiifAsync(idLibro, fechaCorte, reexpresado)), "consultar la situación financiera");

    public Task<ResponseGeneric<FlujoEfectivoIndirectoDTO>> FlujoIndirecto(long idLibro, DateOnly desde, DateOnly hasta)
        => Ejecutar(async () => Traducir(await _api.FlujoIndirectoAsync(idLibro, desde, hasta)), "consultar el flujo de efectivo");

    public Task<ResponseGeneric<EstadoCambiosPatrimonioDTO>> CambiosPatrimonio(long idLibro, DateOnly desde, DateOnly hasta)
        => Ejecutar(async () => Traducir(await _api.CambiosPatrimonioAsync(idLibro, desde, hasta)), "consultar los cambios en el patrimonio");

    public Task<ResponseGeneric<ComparativoNiifDTO>> ComparativoNiif(long idLibro, int ejercicio)
        => Ejecutar(async () => Traducir(await _api.ComparativoNiifAsync(idLibro, ejercicio)), "consultar el comparativo");

    public Task<ResponseGeneric<ICollection<ImportacionSaldosInicialesDTO>>> SaldosIniciales(long idLibro)
        => Ejecutar<ICollection<ImportacionSaldosInicialesDTO>>(async () => TraducirLista(await _api.SaldosInicialesAsync(idLibro)), "consultar las cargas de saldos iniciales");

    public Task<ResponseGeneric<ImportacionSaldosInicialesDTO>> CargarSaldosIniciales(long idLibro, DateOnly fechaCorte, Stream contenido, string nombreArchivo)
        => Ejecutar(async () => Traducir(await _api.CargarSaldosInicialesAsync(idLibro, fechaCorte, contenido, nombreArchivo)), "cargar los saldos iniciales");

    public Task<ResponseGeneric<ImportacionSaldosInicialesDTO>> ObtenerSaldosIniciales(long idImportacion)
        => Ejecutar(async () => Traducir(await _api.ObtenerSaldosInicialesAsync(idImportacion)), "consultar la carga de saldos iniciales");

    public Task<ResponseGeneric<bool>> ConfirmarSaldosIniciales(long idImportacion, string? contrasena)
        => Ejecutar(async () =>
        {
            var r = await _api.ConfirmarSaldosInicialesAsync(idImportacion, new ConfirmarSaldosInicialesDTO { Contrasena = contrasena });
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses is not null);
        }, "confirmar los saldos iniciales");

    public Task<ResponseGeneric<bool>> RechazarSaldosIniciales(long idImportacion, string motivo)
        => Ejecutar(async () => Traducir(await _api.RechazarSaldosInicialesAsync(idImportacion, new RechazarSaldosInicialesDTO { Motivo = motivo })), "rechazar los saldos iniciales");

    public Task<ResponseGeneric<EjecucionConciliacionDTO>> CompararParalelo(long idLibro, DateOnly fechaCorte, Dictionary<string, decimal> saldosReferencia)
        => Ejecutar(async () => Traducir(await _api.CompararParaleloAsync(new ParaleloComandoDTO { IdLibroContable = idLibro, FechaCorte = fechaCorte, SaldosReferencia = saldosReferencia })), "comparar contra el sistema legado");

    public Task<ResponseGeneric<PuestaEnMarchaDTO>> PuestaEnMarcha(long idLibro)
        => Ejecutar(async () => Traducir(await _api.PuestaEnMarchaAsync(idLibro)), "consultar la puesta en marcha");

    public Task<ResponseGeneric<PuestaEnMarchaDTO>> IniciarSombra(long idLibro, int idSucursalPiloto)
        => Ejecutar(async () => Traducir(await _api.IniciarSombraAsync(new IniciarSombraDTO { IdLibroContable = idLibro, IdSucursalPiloto = idSucursalPiloto })), "iniciar el modo sombra");

    public Task<ResponseGeneric<PuestaEnMarchaDTO>> PasarAOficial(long idLibro, string? contrasena)
        => Ejecutar(async () => Traducir(await _api.PasarAOficialAsync(new PasarAOficialDTO { IdLibroContable = idLibro, Contrasena = contrasena })), "pasar el libro a oficial");

    public Task<ResponseGeneric<ICollection<CasoUatDTO>>> CasosUat(long idLibro)
        => Ejecutar<ICollection<CasoUatDTO>>(async () => TraducirLista(await _api.CasosUatAsync(idLibro)), "consultar los casos de la UAT");

    public Task<ResponseGeneric<CasoUatDTO>> ResultadoUat(long idCaso, string estado, string? comentario)
        => Ejecutar(async () => Traducir(await _api.ResultadoUatAsync(idCaso, new RegistrarResultadoUatDTO { Estado = estado, Comentario = comentario })), "registrar el resultado del caso");

    public Task<ResponseGeneric<FirmaUatDTO>> FirmarUat(long idLibro, string? comentario, string? contrasena)
        => Ejecutar(async () => Traducir(await _api.FirmarUatAsync(new FirmarUatDTO { IdLibroContable = idLibro, Comentario = comentario, Contrasena = contrasena })), "firmar la UAT");

    public Task<ResponseGeneric<CorteAlbaranDTO>> SimularCorteAlbaranes(long idLibro, int ejercicio, int mes)
        => Ejecutar(async () => Traducir(await _api.SimularCorteAlbaranesAsync(new EjecutarCorteEntregaDTO { IdLibroContable = idLibro, Ejercicio = ejercicio, Mes = mes })), "simular el corte de albaranes");

    public Task<ResponseGeneric<CorteAlbaranDTO>> EjecutarCorteAlbaranes(long idLibro, int ejercicio, int mes)
        => Ejecutar(async () => Traducir(await _api.EjecutarCorteAlbaranesAsync(new EjecutarCorteEntregaDTO { IdLibroContable = idLibro, Ejercicio = ejercicio, Mes = mes })), "ejecutar el corte de albaranes");

    public Task<ResponseGeneric<ICollection<EvidenciaCierreDTO>>> EvidenciasUat(long idCaso)
        => Ejecutar<ICollection<EvidenciaCierreDTO>>(async () => TraducirLista(await _api.EvidenciasUatAsync(idCaso)), "consultar los soportes del caso");

    public Task<ResponseGeneric<EvidenciaCierreDTO>> AdjuntarEvidenciaUat(long idCaso, Stream contenido, string nombreArchivo, string contentType)
        => Ejecutar(async () => Traducir(await _api.AdjuntarEvidenciaUatAsync(idCaso, contenido, nombreArchivo, contentType)), "adjuntar el soporte del caso");
}
