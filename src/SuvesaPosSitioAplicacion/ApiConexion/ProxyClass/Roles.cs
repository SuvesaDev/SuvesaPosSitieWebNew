using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IRoles" />
public sealed class Roles : ProxyBase, IRoles
{
    private readonly IUsuarioApiCliente _api;

    public Roles(IUsuarioApiCliente api, IContextoSesion sesion, ILogger<Roles> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<Role>>> Buscar()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerRolesAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los roles");

    public Task<ResponseGeneric<ICollection<Modulo>>> Modulos()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerModulosAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los modulos");

    public Task<ResponseGeneric<ICollection<Ventanas>>> Pantallas(int idModulo)
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerVetanasAsync(idModulo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar las pantallas del modulo");

    public Task<ResponseGeneric<ConfiguracionRollDTO>> ObtenerUno(int idRol)
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerConfiguracionPorRolAsync(idRol);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el rol");

    public Task<ResponseGeneric<bool>> Crear(ConfiguracionRollDTO configuracion)
        => Ejecutar(async () =>
        {
            var r = await _api.RegistrarConfiguracionPorRolAsync(configuracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear el rol");

    public Task<ResponseGeneric<bool>> Editar(ConfiguracionRollDTO configuracion)
        => Ejecutar(async () =>
        {
            var r = await _api.EditarConfiguracionPorRolAsync(configuracion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar el rol");

    public Task<ResponseGeneric<Usuario>> ValidarPasswordActual(string contrasena)
        => Ejecutar(async () =>
        {
            var r = await _api.ValidarClaveInternaSinUsuarioAsync(contrasena);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "validar la clave");
}
