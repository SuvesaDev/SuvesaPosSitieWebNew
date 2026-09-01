using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuvesaPosSitioAplicacion.DTOs.Generated;
using SuvesaPosSitioAplicacion.DTOs.Seguridad;

namespace SuvesaPosSitioAplicacion.ApiConexion;

/// <summary>
/// Cliente HTTP tipado para <c>/seguridad/*</c> del API.
///
/// TEMPORAL — hecho a mano porque los contratos NSwag del sitio no se pueden
/// regenerar en local (vienen de otro despliegue). Al regenerarlos, esto lo
/// sustituye el <c>ISeguridadApiCliente</c> generado y este archivo se borra.
///
/// Comparte URL base y <c>ApiAuthHeaderHandler</c> con el resto de clientes
/// (registrado igual en Program.cs). El token lo pone el handler desde
/// <see cref="ContextoLlamada"/>, que llena <c>ProxyBase.Ejecutar</c>.
/// </summary>
public interface ISeguridadApiCliente
{
    Task<SeguridadEnvelope<List<ModuloCatalogoDTO>>> CatalogoAsync();
    Task<SeguridadEnvelope<List<AccionCatalogoDTO>>> AccionesAsync();
    Task<SeguridadEnvelope<AccionCatalogoDTO>> GuardarAccionAsync(AccionCatalogoDTO dto);
    Task<SeguridadEnvelope<ModuloCatalogoDTO>> GuardarModuloAsync(ModuloCatalogoDTO dto);
    Task<SeguridadEnvelope<bool>> DesactivarModuloAsync(int idModulo);
    Task<SeguridadEnvelope<List<FuncionCatalogoDTO>>> FuncionesAsync(int idModulo);
    Task<SeguridadEnvelope<FuncionCatalogoDTO>> GuardarFuncionAsync(FuncionCatalogoDTO dto);
    Task<SeguridadEnvelope<bool>> DesactivarFuncionAsync(int idFuncion);
    Task<SeguridadEnvelope<bool>> AccionesDeFuncionAsync(int idFuncion, List<string> codigos);

    Task<SeguridadEnvelope<List<RolResumenDTO>>> RolesAsync();
    Task<SeguridadEnvelope<RolDetalleDTO>> RolAsync(int idRol);
    Task<SeguridadEnvelope<RolDetalleDTO>> CrearRolAsync(RolDetalleDTO dto);
    Task<SeguridadEnvelope<RolDetalleDTO>> EditarRolAsync(int idRol, RolDetalleDTO dto);
    Task<SeguridadEnvelope<bool>> GuardarPermisosAsync(int idRol, List<PermisoFilaDTO> filas);

    Task<SeguridadEnvelope<List<PerfilSeguridadDTO>>> PerfilesAsync();
    Task<SeguridadEnvelope<PerfilSeguridadDTO>> GuardarPerfilAsync(PerfilSeguridadDTO dto);
    Task<SeguridadEnvelope<PerfilSeguridadDTO>> EditarPerfilAsync(int idPerfil, PerfilSeguridadDTO dto);
    Task<SeguridadEnvelope<bool>> DesactivarPerfilAsync(int idPerfil);

    Task<SeguridadEnvelope<UsuarioAltaDTO>> CrearUsuarioAsync(UsuarioAltaDTO dto);
    Task<SeguridadEnvelope<bool>> CambiarPerfilAsync(long idUsuario, int idPerfil);
    Task<SeguridadEnvelope<bool>> CambiarRolAsync(long idUsuario, int? idRol);
}

/// <summary>Espejo del <c>ResponseGeneric&lt;T&gt;</c> del API (status/currentException/validationErrors/responses).</summary>
public sealed class SeguridadEnvelope<T>
{
    [JsonPropertyName("status")] public ResponseStatus Status { get; set; }
    [JsonPropertyName("currentException")] public string? CurrentException { get; set; }
    [JsonPropertyName("validationErrors")] public List<string>? ValidationErrors { get; set; }
    [JsonPropertyName("responses")] public T? Responses { get; set; }
}

public sealed class SeguridadApiCliente : ISeguridadApiCliente
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public SeguridadApiCliente(HttpClient http) => _http = http;

    private async Task<SeguridadEnvelope<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object? cuerpo = null)
    {
        using var req = new HttpRequestMessage(metodo, ruta);
        if (cuerpo is not null)
        {
            req.Content = JsonContent.Create(cuerpo, options: Json);
        }

        using var resp = await _http.SendAsync(req);
        var texto = await resp.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(texto))
        {
            return new SeguridadEnvelope<T>
            {
                Status = ResponseStatus._1,
                CurrentException = $"El API respondio {(int)resp.StatusCode} sin cuerpo."
            };
        }

        try
        {
            return JsonSerializer.Deserialize<SeguridadEnvelope<T>>(texto, Json)
                   ?? new SeguridadEnvelope<T> { Status = ResponseStatus._1, CurrentException = "Respuesta vacia." };
        }
        catch (JsonException)
        {
            return new SeguridadEnvelope<T>
            {
                Status = ResponseStatus._1,
                CurrentException = $"Respuesta no reconocida del API ({(int)resp.StatusCode})."
            };
        }
    }

    public Task<SeguridadEnvelope<List<ModuloCatalogoDTO>>> CatalogoAsync()
        => EnviarAsync<List<ModuloCatalogoDTO>>(HttpMethod.Get, "seguridad/catalogo");

    public Task<SeguridadEnvelope<List<AccionCatalogoDTO>>> AccionesAsync()
        => EnviarAsync<List<AccionCatalogoDTO>>(HttpMethod.Get, "seguridad/acciones");

    public Task<SeguridadEnvelope<AccionCatalogoDTO>> GuardarAccionAsync(AccionCatalogoDTO dto)
        => EnviarAsync<AccionCatalogoDTO>(HttpMethod.Post, "seguridad/acciones", dto);

    public Task<SeguridadEnvelope<ModuloCatalogoDTO>> GuardarModuloAsync(ModuloCatalogoDTO dto)
        => EnviarAsync<ModuloCatalogoDTO>(HttpMethod.Post, "seguridad/modulos", dto);

    public Task<SeguridadEnvelope<bool>> DesactivarModuloAsync(int idModulo)
        => EnviarAsync<bool>(HttpMethod.Delete, $"seguridad/modulos/{idModulo}");

    public Task<SeguridadEnvelope<List<FuncionCatalogoDTO>>> FuncionesAsync(int idModulo)
        => EnviarAsync<List<FuncionCatalogoDTO>>(HttpMethod.Get, $"seguridad/modulos/{idModulo}/funciones");

    public Task<SeguridadEnvelope<FuncionCatalogoDTO>> GuardarFuncionAsync(FuncionCatalogoDTO dto)
        => EnviarAsync<FuncionCatalogoDTO>(HttpMethod.Post, "seguridad/funciones", dto);

    public Task<SeguridadEnvelope<bool>> DesactivarFuncionAsync(int idFuncion)
        => EnviarAsync<bool>(HttpMethod.Delete, $"seguridad/funciones/{idFuncion}");

    public Task<SeguridadEnvelope<bool>> AccionesDeFuncionAsync(int idFuncion, List<string> codigos)
        => EnviarAsync<bool>(HttpMethod.Put, $"seguridad/funciones/{idFuncion}/acciones", codigos);

    public Task<SeguridadEnvelope<List<RolResumenDTO>>> RolesAsync()
        => EnviarAsync<List<RolResumenDTO>>(HttpMethod.Get, "seguridad/roles");

    public Task<SeguridadEnvelope<RolDetalleDTO>> RolAsync(int idRol)
        => EnviarAsync<RolDetalleDTO>(HttpMethod.Get, $"seguridad/roles/{idRol}");

    public Task<SeguridadEnvelope<RolDetalleDTO>> CrearRolAsync(RolDetalleDTO dto)
        => EnviarAsync<RolDetalleDTO>(HttpMethod.Post, "seguridad/roles", dto);

    public Task<SeguridadEnvelope<RolDetalleDTO>> EditarRolAsync(int idRol, RolDetalleDTO dto)
        => EnviarAsync<RolDetalleDTO>(HttpMethod.Put, $"seguridad/roles/{idRol}", dto);

    public Task<SeguridadEnvelope<bool>> GuardarPermisosAsync(int idRol, List<PermisoFilaDTO> filas)
        => EnviarAsync<bool>(HttpMethod.Put, $"seguridad/roles/{idRol}/permisos", filas);

    public Task<SeguridadEnvelope<List<PerfilSeguridadDTO>>> PerfilesAsync()
        => EnviarAsync<List<PerfilSeguridadDTO>>(HttpMethod.Get, "seguridad/perfiles");

    public Task<SeguridadEnvelope<PerfilSeguridadDTO>> GuardarPerfilAsync(PerfilSeguridadDTO dto)
        => EnviarAsync<PerfilSeguridadDTO>(HttpMethod.Post, "seguridad/perfiles", dto);

    public Task<SeguridadEnvelope<PerfilSeguridadDTO>> EditarPerfilAsync(int idPerfil, PerfilSeguridadDTO dto)
        => EnviarAsync<PerfilSeguridadDTO>(HttpMethod.Put, $"seguridad/perfiles/{idPerfil}", dto);

    public Task<SeguridadEnvelope<bool>> DesactivarPerfilAsync(int idPerfil)
        => EnviarAsync<bool>(HttpMethod.Delete, $"seguridad/perfiles/{idPerfil}");

    public Task<SeguridadEnvelope<UsuarioAltaDTO>> CrearUsuarioAsync(UsuarioAltaDTO dto)
        => EnviarAsync<UsuarioAltaDTO>(HttpMethod.Post, "seguridad/usuarios", dto);

    public Task<SeguridadEnvelope<bool>> CambiarPerfilAsync(long idUsuario, int idPerfil)
        => EnviarAsync<bool>(HttpMethod.Put, $"seguridad/usuarios/{idUsuario}/perfil", new CambioIdDTO { Id = idPerfil });

    public Task<SeguridadEnvelope<bool>> CambiarRolAsync(long idUsuario, int? idRol)
        => EnviarAsync<bool>(HttpMethod.Put, $"seguridad/usuarios/{idUsuario}/rol", new CambioIdDTO { Id = idRol });
}
