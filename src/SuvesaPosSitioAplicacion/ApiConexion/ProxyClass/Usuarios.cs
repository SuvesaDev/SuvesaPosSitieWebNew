using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Seguridad;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IUsuarios" />
public sealed class Usuarios : ProxyBase, IUsuarios
{
    private readonly IUsuarioApiCliente _api;
    private readonly ISeguridadApiCliente _seguridad;
    private readonly IHttpClientFactory _clientes;

    public Usuarios(
        IUsuarioApiCliente api,
        ISeguridadApiCliente seguridad,
        IHttpClientFactory clientes,
        IContextoSesion sesion,
        ILogger<Usuarios> log)
        : base(sesion, log)
    {
        _api = api;
        _seguridad = seguridad;
        _clientes = clientes;
    }

    public Task<ResponseGeneric<ICollection<BuscarUsuarioDTO>>> Buscar(string? texto)
        => Ejecutar(async () =>
        {
            var r = await _api.BuscarUsuariosAsync(new BuscarUsuarioDTO
            {
                Nombre = string.IsNullOrWhiteSpace(texto) ? null : texto
            });

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "buscar usuarios");

    public Task<ResponseGeneric<UsuarioDetalleDTO>> ObtenerUno(string idUsuario)
        => Ejecutar(async () =>
        {
            // El "id" que hace falta mandar aqui es texto (el usuario de acceso), y el
            // cliente generado lo tipa como long? porque asi lo declara el OpenAPI.
            var cliente = _clientes.CreateClient("SeePosApi");
            var respuesta = await cliente.PostAsync(
                $"usuario/ObtenerUnUsuario?id={Uri.EscapeDataString(idUsuario)}", null);

            respuesta.EnsureSuccessStatusCode();

            var r = await respuesta.Content.ReadFromJsonAsync<SeguridadEnvelope<UsuarioDetalleDTO>>()
                     ?? throw new InvalidOperationException("Respuesta vacia del API.");

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "obtener el usuario");

    public Task<ResponseGeneric<UsuarioAltaDTO>> Crear(UsuarioAltaDTO usuario)
        => Ejecutar(async () =>
        {
            var r = await _seguridad.CrearUsuarioAsync(usuario);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear el usuario");

    public Task<ResponseGeneric<UsuarioDTO>> Editar(long id, UsuarioDTO usuario)
        => Ejecutar(async () =>
        {
            var r = await _api.ModificarUsuarioAsync(id, usuario);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar el usuario");

    public Task<ResponseGeneric<bool>> CambiarPerfil(long id, int idPerfil)
        => Ejecutar(async () =>
        {
            var r = await _seguridad.CambiarPerfilAsync(id, idPerfil);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "cambiar el perfil del usuario");

    public Task<ResponseGeneric<bool>> CambiarRol(long id, int? idRol)
        => Ejecutar(async () =>
        {
            var r = await _seguridad.CambiarRolAsync(id, idRol);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "cambiar el rol del usuario");
}
