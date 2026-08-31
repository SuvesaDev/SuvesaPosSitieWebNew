using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Seguridad;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IPerfiles" />
public sealed class Perfiles : ProxyBase, IPerfiles
{
    private readonly ISeguridadApiCliente _api;

    public Perfiles(ISeguridadApiCliente api, IContextoSesion sesion, ILogger<Perfiles> log)
        : base(sesion, log)
    {
        _api = api;
    }

    public Task<ResponseGeneric<ICollection<PerfilSeguridadDTO>>> Listar()
        => Ejecutar<ICollection<PerfilSeguridadDTO>>(async () =>
        {
            var r = await _api.PerfilesAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors,
                (ICollection<PerfilSeguridadDTO>?)r.Responses);
        }, "consultar los perfiles");

    public Task<ResponseGeneric<PerfilSeguridadDTO>> Crear(PerfilSeguridadDTO perfil)
        => Ejecutar(async () =>
        {
            var r = await _api.GuardarPerfilAsync(perfil);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear el perfil");

    public Task<ResponseGeneric<PerfilSeguridadDTO>> Editar(int idPerfil, PerfilSeguridadDTO perfil)
        => Ejecutar(async () =>
        {
            var r = await _api.EditarPerfilAsync(idPerfil, perfil);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar el perfil");
}
