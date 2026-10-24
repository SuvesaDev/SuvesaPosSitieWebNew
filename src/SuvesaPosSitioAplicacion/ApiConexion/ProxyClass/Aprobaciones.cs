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
}
