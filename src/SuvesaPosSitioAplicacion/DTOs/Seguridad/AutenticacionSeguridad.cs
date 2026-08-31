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

    [JsonPropertyName("permisos")]
    public List<PermisoLoginDTO>? Permisos { get; set; }
}
