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
}
