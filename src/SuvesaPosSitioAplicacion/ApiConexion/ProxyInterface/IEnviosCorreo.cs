using SuvesaPosSitioAplicacion.DTOs.Correo;
using SuvesaPosSitioAplicacion.Helpers;

namespace SuvesaPosSitioAplicacion.ApiConexion.ProxyInterface;

/// <summary>Bandeja de envíos de correo de comprobantes (MOTOR_CORREO_COMPROBANTES_WEB.md §4).</summary>
public interface IEnviosCorreo
{
    Task<ResponseGeneric<PaginaEnviosCorreoDTO>> Listar(
        string? estado, int? idEmisor, DateTime? desde, DateTime? hasta, string? texto, int pagina, int tamano);

    /// <summary>Reencola el envío. Con <paramref name="destinatarios"/> lo reenvía solo esta vez a esos correos (separados por ';'); sin él, a los mismos.</summary>
    Task<ResponseGeneric<bool>> Reenviar(string clave, string? destinatarios = null);
}
