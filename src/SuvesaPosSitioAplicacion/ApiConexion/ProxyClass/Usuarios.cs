using System.Net.Http.Json;
using SuvesaPosSitioAplicacion.ApiConexion.Generated;
using SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.Helpers;
using SuvesaPosSitioAplicacion.Security;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyClass;

/// <inheritdoc cref="IUsuarios" />
public sealed class Usuarios : ProxyBase, IUsuarios
{
    private readonly IUsuarioApiCliente _api;
    private readonly IHttpClientFactory _clientes;

    public Usuarios(
        IUsuarioApiCliente api,
        IHttpClientFactory clientes,
        IContextoSesion sesion,
        ILogger<Usuarios> log)
        : base(sesion, log)
    {
        _api = api;
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

    public Task<ResponseGeneric<UsuariosDTO>> ObtenerUno(string idUsuario)
        => Ejecutar(async () =>
        {
            // Llamada manual, no con el cliente generado: el "id" que hace falta
            // mandar aqui es texto (el usuario de acceso), y el cliente generado
            // tipa ese parametro como long? porque asi lo declara el OpenAPI.
            var cliente = _clientes.CreateClient("SeePosApi");
            var respuesta = await cliente.PostAsync(
                $"usuario/ObtenerUnUsuario?id={Uri.EscapeDataString(idUsuario)}", null);

            respuesta.EnsureSuccessStatusCode();

            var r = await respuesta.Content.ReadFromJsonAsync<UsuariosDTOResponseGeneric>()
                     ?? throw new InvalidOperationException("Respuesta vacia del API.");

            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "obtener el usuario");

    public Task<ResponseGeneric<UsuariosDTO>> Crear(UsuariosDTO usuario)
        => Ejecutar(async () =>
        {
            var r = await _api.CrearUsuarioAsync(usuario);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "crear el usuario");

    public Task<ResponseGeneric<UsuarioDTO>> Editar(long id, UsuarioDTO usuario)
        => Ejecutar(async () =>
        {
            var r = await _api.ModificarUsuarioAsync(id, usuario);
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "editar el usuario");

    public Task<ResponseGeneric<ICollection<Perfil>>> ObtenerPerfiles()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerPerfilesAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los perfiles");

    public Task<ResponseGeneric<ICollection<Role>>> ObtenerRoles()
        => Ejecutar(async () =>
        {
            var r = await _api.ObtenerRolesAsync();
            return EnvelopeApi.A(r.Status, r.CurrentException, r.ValidationErrors, r.Responses);
        }, "consultar los roles");
}
