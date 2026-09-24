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
}
