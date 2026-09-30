using System.Text.Json.Serialization;

namespace SuvesaPosSitioAplicacion.DTOs.Generated;

// El listado de usuarios marca quién tiene el ingreso bloqueado por intentos.
// El contrato generado no trae el campo; al regenerar NSwag este archivo se quita
// si el DTO generado ya incluye `bloqueado`.

public partial class BuscarUsuarioDTO
{
    [JsonPropertyName("bloqueado")]
    public bool Bloqueado { get; set; }
}
