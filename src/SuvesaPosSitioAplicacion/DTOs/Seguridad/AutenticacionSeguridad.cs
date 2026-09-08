using System.Text.Json.Serialization;
using SuvesaPosSitioAplicacion.DTOs.Seguridad;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

/// <summary>
/// Extension del <c>Autenticacion</c> generado por NSwag con los campos del rediseno
/// de seguridad V2. El cliente NSwag deserializa por reflexion con System.Text.Json,
/// asi que estas propiedades parciales se rellenan solas desde el JSON de la respuesta.
///
/// TEMPORAL: al regenerar los contratos contra el API nuevo, NSwag emitira estos
/// campos en la clase generada y este archivo se borra.
/// </summary>
public partial class Autenticacion
{
    [JsonPropertyName("perfil")]
    public PerfilLoginDTO? Perfil { get; set; }

    /// <summary>Nombre para mostrar del usuario (menú de la barra superior).</summary>
    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("iniciales")]
    public string? Iniciales { get; set; }

    [JsonPropertyName("permisos")]
    public List<PermisoLoginDTO>? Permisos { get; set; }

    /// <summary>Compat plano: capacidad del perfil (permitir existencia negativa).</summary>
    [JsonPropertyName("permiteExistenciaNegativa")]
    public bool PermiteExistenciaNegativa { get; set; }
}
