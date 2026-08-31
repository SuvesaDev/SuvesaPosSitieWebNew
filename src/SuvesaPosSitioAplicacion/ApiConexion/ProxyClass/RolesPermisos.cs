using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Seguridad;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IRolesPermisos" />
public sealed class RolesPermisos : ProxyBase, IRolesPermisos
{
    private readonly ISeguridadApiCliente _api;
    private readonly IUsuarioApiCliente _usuario;

    public RolesPermisos(
        ISeguridadApiCliente api,
        IUsuarioApiCliente usuario,
        IContextoSesion sesion,
        ILogger<RolesPermisos> log)
        : base(sesion, log)
    {
        _api = api;
        _usuario = usuario;
    }

    public Task<ResponseGeneric<ICollection<ModuloCatalogoDTO>>> Catalogo()
        => Ejecutar<ICollection<ModuloCatalogoDTO>>(async () =>
        {
            var r = await _api.CatalogoAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors,
                (ICollection<ModuloCatalogoDTO>?)r.Responses);
        }, "consultar el catalogo de seguridad");

    public Task<ResponseGeneric<ICollection<AccionCatalogoDTO>>> Acciones()
        => Ejecutar<ICollection<AccionCatalogoDTO>>(async () =>
        {
            var r = await _api.AccionesAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors,
                (ICollection<AccionCatalogoDTO>?)r.Responses);
        }, "consultar las acciones");

    public Task<ResponseGeneric<AccionCatalogoDTO>> GuardarAccion(AccionCatalogoDTO dto)
        => Ejecutar(async () =>
        {
            var r = await _api.GuardarAccionAsync(dto);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "guardar la accion");

    public Task<ResponseGeneric<ModuloCatalogoDTO>> GuardarModulo(ModuloCatalogoDTO dto)
        => Ejecutar(async () =>
        {
            var r = await _api.GuardarModuloAsync(dto);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "guardar el modulo");

    public Task<ResponseGeneric<bool>> DesactivarModulo(int idModulo)
        => Ejecutar(async () =>
        {
            var r = await _api.DesactivarModuloAsync(idModulo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "desactivar el modulo");

    public Task<ResponseGeneric<ICollection<FuncionCatalogoDTO>>> Funciones(int idModulo)
        => Ejecutar<ICollection<FuncionCatalogoDTO>>(async () =>
        {
            var r = await _api.FuncionesAsync(idModulo);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors,
                (ICollection<FuncionCatalogoDTO>?)r.Responses);
        }, "consultar las funciones del modulo");

    public Task<ResponseGeneric<FuncionCatalogoDTO>> GuardarFuncion(FuncionCatalogoDTO dto)
        => Ejecutar(async () =>
        {
            var r = await _api.GuardarFuncionAsync(dto);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "guardar la funcion");

    public Task<ResponseGeneric<bool>> DesactivarFuncion(int idFuncion)
        => Ejecutar(async () =>
        {
            var r = await _api.DesactivarFuncionAsync(idFuncion);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "desactivar la funcion");

    public Task<ResponseGeneric<bool>> GuardarAccionesDeFuncion(int idFuncion, IEnumerable<string> codigos)
        => Ejecutar(async () =>
        {
            var r = await _api.AccionesDeFuncionAsync(idFuncion, codigos.ToList());
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "guardar las acciones de la funcion");

    public Task<ResponseGeneric<ICollection<RolResumenDTO>>> Roles()
        => Ejecutar<ICollection<RolResumenDTO>>(async () =>
        {
            var r = await _api.RolesAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors,
                (ICollection<RolResumenDTO>?)r.Responses);
        }, "consultar los roles");

    public Task<ResponseGeneric<RolDetalleDTO>> Rol(int idRol)
        => Ejecutar(async () =>
        {
            var r = await _api.RolAsync(idRol);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar el rol");

    public Task<ResponseGeneric<RolDetalleDTO>> CrearRol(RolDetalleDTO dto)
        => Ejecutar(async () =>
        {
            var r = await _api.CrearRolAsync(dto);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear el rol");

    public Task<ResponseGeneric<RolDetalleDTO>> EditarRol(int idRol, RolDetalleDTO dto)
        => Ejecutar(async () =>
        {
            var r = await _api.EditarRolAsync(idRol, dto);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar el rol");

    public Task<ResponseGeneric<bool>> GuardarPermisos(int idRol, IEnumerable<PermisoFilaDTO> filas)
        => Ejecutar(async () =>
        {
            var r = await _api.GuardarPermisosAsync(idRol, filas.ToList());
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "guardar los permisos del rol");

    public Task<ResponseGeneric<Usuario>> ValidarPasswordActual(string contrasena)
        => Ejecutar(async () =>
        {
            var r = await _usuario.ValidarClaveInternaSinUsuarioAsync(contrasena);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "validar la clave");
}
